using System.IO;

namespace thelebaron.BuildTools
{
    public static class FileManagement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">The folder and its contents to delete</param>
        /// <param name="contentsOnly">Prevents from deleting the root folder specified(only delete contents)</param>
        public static void DeleteDirectory(string target, bool contentsOnly = false)
        {
            string[] files = Directory.GetFiles(target);
            string[] dir   = Directory.GetDirectories(target);
 
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
 
            foreach (string path in dir)
            {
                DeleteDirectory(path);
            }
 
            if(!contentsOnly)
                Directory.Delete(target, true);
        }
    }
}