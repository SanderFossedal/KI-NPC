using UnityEditor;

namespace LeastSquares.Overtone
{
    [InitializeOnLoad]
    public class InstallationWindowLoader
    {
        private const string ProjectOpenedKey = "ProjectOpened";

        static InstallationWindowLoader()
        {
            EditorApplication.delayCall += ShowCustomMenuWindow;
        }

        private static void ShowCustomMenuWindow()
        {
            var value = EditorPrefs.GetBool(ProjectOpenedKey);
            if (!value)
            {
                OvertoneGettingStartedWindow.ShowWindow();
                EditorPrefs.SetBool(ProjectOpenedKey, true);
            }
        }
    }
}