// WasmLoaderPlugin.jslib
mergeInto(LibraryManager.library, {
    // WASM 모듈 로더 초기화 (한 번만 호출)
    InitWasmLoader: function() {
        if (window.__wasmLoader) {
            return; // 이미 초기화됨
        }
        
        window.__wasmLoader = {
            loadedModules: new Map(),
            loadingModules: new Map(),
            callbacks: new Map()
        };
        
        console.log('[WASM] Loader initialized');
    },
    
    // WASM 모듈 로드 (비동기)
    LoadWasmModule: function(moduleNamePtr, moduleUrlPtr, gameObjectPtr, callbackMethodPtr) {
        var moduleName = UTF8ToString(moduleNamePtr);
        var moduleUrl = UTF8ToString(moduleUrlPtr);
        var gameObject = UTF8ToString(gameObjectPtr);
        var callbackMethod = UTF8ToString(callbackMethodPtr);
        
        if (!window.__wasmLoader) {
            console.error('[WASM] Loader not initialized. Call InitWasmLoader first.');
            if (gameObject && callbackMethod) {
                try { SendMessage(gameObject, callbackMethod, 'error:not_initialized'); } catch(e) { console.log(e); }
            }
            return;
        }
        
        var loader = window.__wasmLoader;
        
        // 이미 로드된 모듈
        if (loader.loadedModules.has(moduleName)) {
            console.log(`[WASM] Module already loaded: ${moduleName}`);
            if (gameObject && callbackMethod) {
                try { SendMessage(gameObject, callbackMethod, 'success:already_loaded'); } catch(e) { console.log(e); }
            }
            return;
        }
        
        // 이미 로딩 중인 모듈
        if (loader.loadingModules.has(moduleName)) {
            console.log(`[WASM] Module already loading: ${moduleName}`);
            // 콜백을 추가 등록
            if (gameObject && callbackMethod) {
                var callbacks = loader.callbacks.get(moduleName) || [];
                callbacks.push({ gameObject: gameObject, callbackMethod: callbackMethod });
                loader.callbacks.set(moduleName, callbacks);
            }
            return;
        }
        
        console.log(`[WASM] Loading module: ${moduleName} from ${moduleUrl}`);
        
        // 콜백 등록
        if (gameObject && callbackMethod) {
            loader.callbacks.set(moduleName, [{ gameObject: gameObject, callbackMethod: callbackMethod }]);
        }
        
        // 로딩 프로미스 생성
        var loadPromise = fetch(moduleUrl)
            .then(function(response) {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.arrayBuffer();
            })
            .then(function(arrayBuffer) {
                return WebAssembly.compile(arrayBuffer);
            })
            .then(function(wasmModule) {
                return WebAssembly.instantiate(wasmModule);
            })
            .then(function(wasmInstance) {
                // Unity WebGL 런타임에 모듈 등록
                if (window.unityInstance && window.unityInstance.Module) {
                    window.unityInstance.Module.wasmModules = window.unityInstance.Module.wasmModules || {};
                    window.unityInstance.Module.wasmModules[moduleName] = wasmInstance;
                }
                
                // 로드 완료
                loader.loadedModules.set(moduleName, wasmInstance);
                loader.loadingModules.delete(moduleName);
                
                console.log(`[WASM] Module loaded successfully: ${moduleName}`);
                
                // 모든 콜백 호출
                var callbacks = loader.callbacks.get(moduleName) || [];
                callbacks.forEach(function(cb) {
                    try { 
                        SendMessage(cb.gameObject, cb.callbackMethod, 'success:loaded'); 
                    } catch(e) { 
                        console.log(e); 
                    }
                });
                loader.callbacks.delete(moduleName);
                
                return wasmInstance;
            })
            .catch(function(error) {
                console.error(`[WASM] Failed to load module: ${moduleName}`, error);
                loader.loadingModules.delete(moduleName);
                
                // 모든 콜백에 에러 전달
                var callbacks = loader.callbacks.get(moduleName) || [];
                callbacks.forEach(function(cb) {
                    try { 
                        SendMessage(cb.gameObject, cb.callbackMethod, 'error:' + error.message); 
                    } catch(e) { 
                        console.log(e); 
                    }
                });
                loader.callbacks.delete(moduleName);
            });
        
        loader.loadingModules.set(moduleName, loadPromise);
    },
    
    // WASM 모듈 상태 확인 (동기)
    GetWasmModuleStatus: function(moduleNamePtr) {
        var moduleName = UTF8ToString(moduleNamePtr);
        
        if (!window.__wasmLoader) {
            return allocate(intArrayFromString('not_initialized'), 'i8', ALLOC_NORMAL);
        }
        
        var loader = window.__wasmLoader;
        var status;
        
        if (loader.loadedModules.has(moduleName)) {
            status = 'loaded';
        } else if (loader.loadingModules.has(moduleName)) {
            status = 'loading';
        } else {
            status = 'not_loaded';
        }
        
        return allocate(intArrayFromString(status), 'i8', ALLOC_NORMAL);
    },
    
    // WASM 모듈이 로드되었는지 확인 (동기)
    IsWasmModuleLoaded: function(moduleNamePtr) {
        var moduleName = UTF8ToString(moduleNamePtr);
        
        if (!window.__wasmLoader) {
            return 0; // false
        }
        
        return window.__wasmLoader.loadedModules.has(moduleName) ? 1 : 0;
    },
    
    // 로드된 모든 모듈 이름 가져오기 (JSON 배열 형태)
    GetLoadedModuleNames: function() {
        if (!window.__wasmLoader) {
            return allocate(intArrayFromString('[]'), 'i8', ALLOC_NORMAL);
        }
        
        var moduleNames = Array.from(window.__wasmLoader.loadedModules.keys());
        var json = JSON.stringify(moduleNames);
        
        return allocate(intArrayFromString(json), 'i8', ALLOC_NORMAL);
    },
    
    // 특정 모듈 언로드
    UnloadWasmModule: function(moduleNamePtr) {
        var moduleName = UTF8ToString(moduleNamePtr);
        
        if (!window.__wasmLoader) {
            return 0; // false
        }
        
        var loader = window.__wasmLoader;
        
        if (loader.loadedModules.has(moduleName)) {
            loader.loadedModules.delete(moduleName);
            
            // Unity WebGL 런타임에서도 제거
            if (window.unityInstance && 
                window.unityInstance.Module && 
                window.unityInstance.Module.wasmModules) {
                delete window.unityInstance.Module.wasmModules[moduleName];
            }
            
            console.log(`[WASM] Module unloaded: ${moduleName}`);
            return 1; // true
        }
        
        return 0; // false
    }
});