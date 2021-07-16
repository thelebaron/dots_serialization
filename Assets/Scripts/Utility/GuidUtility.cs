using System;
using System.Security.Cryptography;
using System.Text;
using Unity.Entities;

namespace Utility
{
    public static class GuidUtility
    {
        public static unsafe Hash128 GenerateGuid(object obj)
        {
            Guid guid;
            var  input = obj.ToString() + obj.GetType();
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytehash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                guid = new Guid(bytehash);
            }
        
            var hash = new Unity.Entities.Hash128();
            hash = *(Hash128*)&guid;
            return hash;
        }
    }
}