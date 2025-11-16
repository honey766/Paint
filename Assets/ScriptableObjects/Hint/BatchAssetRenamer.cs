#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class BatchAssetRenamer : EditorWindow
{
    // 메뉴에 "Assets/Batch Rename 'Stage' to 'Hint'" 항목을 추가합니다.
    [MenuItem("Assets/Batch Rename 'Stage' to 'Hint'")]
    private static void RenameStageToHint()
    {
        // 현재 선택된 모든 오브젝트(에셋)를 가져옵니다.
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("이름을 변경할 에셋 파일을 선택해주세요.");
            return;
        }

        // 에셋 변경 시작
        AssetDatabase.StartAssetEditing();

        int renamedCount = 0;
        try
        {
            foreach (Object obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                // .asset 확장자를 포함한 전체 파일 이름이 아닌, 순수 파일 이름만 가져옵니다.
                string fileName = Path.GetFileNameWithoutExtension(assetPath);

                // 파일 이름에 "Stage"가 포함되어 있는지 확인
                if (fileName.Contains("Stage"))
                {
                    string newName = fileName.Replace("Stage", "Hint");

                    // 에셋(파일) 이름 변경
                    string error = AssetDatabase.RenameAsset(assetPath, newName);

                    if (string.IsNullOrEmpty(error))
                    {
                        renamedCount++;
                    }
                    else
                    {
                        Debug.LogError($"'{fileName}' 이름 변경 실패: {error}");
                    }
                }
            }
        }
        finally
        {
            // 에셋 변경 완료 및 데이터베이스 새로고침
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"{renamedCount}개의 에셋 이름을 'Stage'에서 'Hint'로 변경했습니다.");
        // C# 스크립트와 달리 후속 작업이 필요 없으므로 완료 메시지만 띄웁니다.
        EditorUtility.DisplayDialog("일괄 변경 완료", $"{renamedCount}개의 에셋 이름을 성공적으로 변경했습니다.", "확인");
    }

    // 메뉴 유효성 검사 (선택된 파일이 있을 때만 메뉴 활성화)
    [MenuItem("Assets/Batch Rename 'Stage' to 'Hint'", true)]
    private static bool ValidateRename()
    {
        return Selection.objects != null && Selection.objects.Length > 0;
    }
}
#endif