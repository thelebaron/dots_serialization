using System.IO;

namespace Utility
{
    /// <summary>
    /// https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
    /// </summary>
    public static class FileHelper
    {
        public static bool IsFileLocked(FileInfo file)
        {

            
            try
            {
                using(FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                
                return true;
            }

            //file is not locked
            return false;
        }  
    }
}