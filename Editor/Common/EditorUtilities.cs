using System.Threading.Tasks;

namespace UnityEditor
{
    public static class EditorUtilities
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that completes on the next editor frame.
        /// </summary>
        /// <remarks>
        /// This method is useful for awaiting a single frame delay in editor code.
        /// It hooks into <see cref="EditorApplication.delayCall"/> and completes
        /// the task when the editor processes the next frame.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that completes on the next editor frame.</returns>
        public static Task NextEditorFrame()
        {
            var tcs = new TaskCompletionSource<bool>();
            EditorApplication.delayCall += () => tcs.SetResult(true);
            return tcs.Task;
        }
    }
}