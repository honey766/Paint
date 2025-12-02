/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WasmSplitManager : MonoBehaviour
{
    public static WasmSplitManager Instance { get; private set; }
    
    [System.Serializable]
    public class WasmModule
    {
        public string moduleName;
        public string moduleUrl;
        public ModulePriority priority;
        public string[] dependencies;
        public bool preload = false;
        public float estimatedSizeMB;
        public ModuleState state = ModuleState.NotLoaded;
    }
    
    public enum ModulePriority
    {
        Critical = 0,  // 게임 시작 전 필수
        High = 1,      // 초기 로딩 시 필요
        Medium = 2,    // 기능 사용 시 로딩
        Low = 3,       // 선택적 로딩
        OnDemand = 4   // 요청 시에만 로딩
    }
    
    public enum ModuleState
    {
        NotLoaded,
        Loading,
        Loaded,
        Failed
    }
    
    [Header("WASM 모듈 설정")]
    public WasmModule[] wasmModules;
    
    [Header("로딩 설정")]
    public bool enableAsyncLoading = true;
    public int maxConcurrentLoads = 2;
    public float loadTimeoutSeconds = 30f;
    
    // 내부 상태
    private Dictionary<string, WasmModule> moduleMap = new Dictionary<string, WasmModule>();
    private HashSet<string> loadingModules = new HashSet<string>();
    private Queue<WasmModule> loadQueue = new Queue<WasmModule>();
    private int activeLoads = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWasmSplit();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeWasmSplit()
    {
        // 모듈 맵 생성
        foreach (var module in wasmModules)
        {
            moduleMap[module.moduleName] = module;
        }
        
        // Critical 모듈들 즉시 로딩 시작
        LoadCriticalModules();
        
        // 백그라운드 로딩 시작
        StartCoroutine(ProcessLoadQueue());
        
        Debug.Log("WASM 코드 분할 시스템 초기화 완료");
    }
    
    void LoadCriticalModules()
    {
        var criticalModules = System.Array.FindAll(wasmModules, 
            m => m.priority == ModulePriority.Critical);
        
        foreach (var module in criticalModules)
        {
            QueueModuleLoad(module.moduleName);
        }
        
        Debug.Log($"Critical 모듈 로딩 시작: {criticalModules.Length}개");
    }
    
    public void QueueModuleLoad(string moduleName)
    {
        if (moduleMap.ContainsKey(moduleName) && 
            moduleMap[moduleName].state == ModuleState.NotLoaded &&
            !loadingModules.Contains(moduleName))
        {
            loadQueue.Enqueue(moduleMap[moduleName]);
        }
    }
    
    IEnumerator ProcessLoadQueue()
    {
        while (true)
        {
            while (loadQueue.Count > 0 && activeLoads < maxConcurrentLoads)
            {
                var module = loadQueue.Dequeue();
                StartCoroutine(LoadWasmModule(module));
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    IEnumerator LoadWasmModule(WasmModule module)
    {
        loadingModules.Add(module.moduleName);
        module.state = ModuleState.Loading;
        activeLoads++;
        
        Debug.Log($"WASM 모듈 로딩 시작: {module.moduleName}");
        float startTime = Time.realtimeSinceStartup;
        
        // 의존성 체크
        yield return StartCoroutine(EnsureDependencies(module));
        
        // 실제 모듈 로딩
        bool loadSuccess = false;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL에서 실제 WASM 모듈 로딩
        //loadSuccess = yield return StartCoroutine(LoadWasmModuleWebGL(module));
        yield return StartCoroutine(LoadWasmModuleWebGL(module));
        loadSuccess = true;
#else
        // 에디터/다른 플랫폼에서는 시뮬레이션
        yield return new WaitForSeconds(0.5f); // 로딩 시뮬레이션
        loadSuccess = true;
#endif
        
        // 로딩 결과 처리
        float loadTime = Time.realtimeSinceStartup - startTime;
        
        if (loadSuccess)
        {
            module.state = ModuleState.Loaded;
            OnModuleLoadSuccess(module, loadTime);
        }
        else
        {
            module.state = ModuleState.Failed;
            OnModuleLoadFailed(module, loadTime);
        }
        
        loadingModules.Remove(module.moduleName);
        activeLoads--;
    }
    
    IEnumerator EnsureDependencies(WasmModule module)
    {
        foreach (var dependency in module.dependencies)
        {
            if (moduleMap.ContainsKey(dependency))
            {
                var depModule = moduleMap[dependency];
                
                if (depModule.state == ModuleState.NotLoaded)
                {
                    // 의존성 모듈을 먼저 로딩
                    yield return StartCoroutine(LoadWasmModule(depModule));
                }
                else if (depModule.state == ModuleState.Loading)
                {
                    // 의존성 모듈 로딩 완료 대기
                    while (depModule.state == ModuleState.Loading)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                
                if (depModule.state == ModuleState.Failed)
                {
                    Debug.LogError($"의존성 모듈 로딩 실패: {dependency}");
                    yield break;
                }
            }
        }
    }
    
#if UNITY_WEBGL && !UNITY_EDITOR
    IEnumerator LoadWasmModuleWebGL(WasmModule module)
    {
        // JavaScript와 상호작용하여 WASM 모듈 로딩
        string loadCommand = $"loadWasmModule('{module.moduleName}', '{module.moduleUrl}')";
        
        // JS 함수 호출
        Application.ExternalCall("eval", loadCommand);
        
        // 로딩 완료 대기
        float timeout = Time.realtimeSinceStartup + loadTimeoutSeconds;
        
        while (Time.realtimeSinceStartup < timeout)
        {
            // JS에서 로딩 상태 확인
            string status = GetWasmModuleStatus(module.moduleName);
            
            if (status == "loaded")
            {
                yield return true;
            }
            else if (status == "failed")
            {
                yield return false;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // 타임아웃
        Debug.LogError($"WASM 모듈 로딩 타임아웃: {module.moduleName}");
        yield return false;
    }
    
    [DllImport("__Internal")]
    private static extern string GetWasmModuleStatus(string moduleName);
#endif
    
    void OnModuleLoadSuccess(WasmModule module, float loadTime)
    {
        Debug.Log($"WASM 모듈 로딩 성공: {module.moduleName} ({loadTime:F2}초)");
        
        // // 성공 이벤트 발송
        // AppsInToss.SendEvent("wasm_module_loaded", new Dictionary<string, object>
        // {
        //     {"module_name", module.moduleName},
        //     {"load_time", loadTime},
        //     {"estimated_size_mb", module.estimatedSizeMB}
        // });
        
        // 분석 데이터 전송
        SendModuleAnalytics(module, true, loadTime);
        
        // High 우선순위 모듈들 자동 로딩
        TriggerHighPriorityLoading();
    }
    
    void OnModuleLoadFailed(WasmModule module, float loadTime)
    {
        Debug.LogError($"WASM 모듈 로딩 실패: {module.moduleName}");
        
        // // 실패 이벤트 발송
        // AppsInToss.SendEvent("wasm_module_failed", new Dictionary<string, object>
        // {
        //     {"module_name", module.moduleName},
        //     {"load_time", loadTime}
        // });
        
        // 분석 데이터 전송
        SendModuleAnalytics(module, false, loadTime);
        
        // 재시도 로직 (중요한 모듈만)
        if (module.priority <= ModulePriority.High)
        {
            StartCoroutine(RetryModuleLoad(module, 5f));
        }
    }
    
    void TriggerHighPriorityLoading()
    {
        var highPriorityModules = System.Array.FindAll(wasmModules,
            m => m.priority == ModulePriority.High && 
                 m.state == ModuleState.NotLoaded);
        
        foreach (var module in highPriorityModules)
        {
            QueueModuleLoad(module.moduleName);
        }
    }
    
    IEnumerator RetryModuleLoad(WasmModule module, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        module.state = ModuleState.NotLoaded;
        QueueModuleLoad(module.moduleName);
        
        Debug.Log($"WASM 모듈 재시도: {module.moduleName}");
    }
    
    void SendModuleAnalytics(WasmModule module, bool success, float loadTime)
    {
        // var analyticsData = new Dictionary<string, object>
        // {
        //     {"module_name", module.moduleName},
        //     {"priority", module.priority.ToString()},
        //     {"estimated_size_mb", module.estimatedSizeMB},
        //     {"success", success},
        //     {"load_time", loadTime},
        //     {"device_model", SystemInfo.deviceModel},
        //     {"browser_info", GetBrowserInfo()},
        //     {"timestamp", System.DateTime.UtcNow.ToString("o")}
        // };
        
        // AppsInToss.SendAnalytics("wasm_module_load", analyticsData);
    }
    
    string GetBrowserInfo()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return Application.ExternalEval("navigator.userAgent");
#else
        return "Editor";
#endif
    }
    
    // 공개 API
    public bool IsModuleLoaded(string moduleName)
    {
        return moduleMap.ContainsKey(moduleName) && 
               moduleMap[moduleName].state == ModuleState.Loaded;
    }
    
    public bool IsModuleLoading(string moduleName)
    {
        return loadingModules.Contains(moduleName);
    }
    
    public ModuleState GetModuleState(string moduleName)
    {
        return moduleMap.ContainsKey(moduleName) ? 
               moduleMap[moduleName].state : 
               ModuleState.NotLoaded;
    }
    
    public void RequestModuleLoad(string moduleName)
    {
        if (moduleMap.ContainsKey(moduleName))
        {
            var module = moduleMap[moduleName];
            if (module.priority >= ModulePriority.Medium)
            {
                QueueModuleLoad(moduleName);
            }
        }
    }
    
    public float GetTotalLoadProgress()
    {
        int totalModules = wasmModules.Length;
        int loadedModules = 0;
        
        foreach (var module in wasmModules)
        {
            if (module.state == ModuleState.Loaded)
            {
                loadedModules++;
            }
        }
        
        return totalModules > 0 ? (float)loadedModules / totalModules : 1f;
    }
    
    public string[] GetLoadedModules()
    {
        var loadedModules = new List<string>();
        
        foreach (var kvp in moduleMap)
        {
            if (kvp.Value.state == ModuleState.Loaded)
            {
                loadedModules.Add(kvp.Key);
            }
        }
        
        return loadedModules.ToArray();
    }
}
*/