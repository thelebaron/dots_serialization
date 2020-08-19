using UnityEditor;

namespace thelebaron.BuildTools 
{
    public static class GitCommands
    {
        
        /// <summary>
        /// https://stackoverflow.com/questions/572549/difference-between-git-add-a-and-git-add
        /// 
        /// </summary>
        [MenuItem("Git/Stage All")]
        public static void StageAll()
        {
            SemVer.BumpPatch();
            Git.Run("add -A");
        }
        
        public static void Stash()
        {
            SemVer.BumpPatch();
            Git.Run("stash");
        }
        
        

    }
}