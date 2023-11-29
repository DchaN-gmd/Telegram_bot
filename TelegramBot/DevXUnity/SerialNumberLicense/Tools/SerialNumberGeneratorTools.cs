using System;
using System.IO;
using System.Collections.Generic;
using DevXUnity.SerialNumberLicense.Editor;


    public class SerialNumberGeneratorTools
    {
        public static string BasePathToLicenses = "AllSNLicenses";
        
        internal static List<LicenseInfo> LicenseList;

        /// <summary>
        /// Base path to licenses
        /// </summary>
        internal static string BasePath
        {
            get
            {
                if (Directory.Exists(BasePathToLicenses) == false)
                    Directory.CreateDirectory(BasePathToLicenses);
                return BasePathToLicenses;
            }
        }
        
        /// <summary>
        /// RSA close key
        /// </summary>
        public static string CloseKey
        {
            get;
            set;
        }

        /// <summary>
        /// RSA open key 
        /// </summary>
        public static string OpenKey
        {
            get;
            set;
        }

        public static void SetOpenAndCloseKeyDirectory(string CloseKeyPath, string OpenKeyPath)
        {
            CloseKey = CloseKeyPath;
            OpenKey = OpenKeyPath;
        }

    internal static void UpdateKeys(bool reCreate = false, bool asDsa = false)
        {
            if (CloseKey != null && !reCreate) return;
            
            string config;
            var configName = Path.Combine(BasePath, "SerialNumberSigner.config");

            if (File.Exists(configName)) File.ReadAllText(configName);
            SerialNumberSigner signer;

#if !UNITY_WSA
            if (asDsa)
            {
                signer = SerialNumberSigner.CreateDsa();
                config = "DSA";
            }
            else
#endif
            {
                signer = SerialNumberSigner.CreateSimple();
                config = "Simple" + "\n" + "8" + "\n" + "10000";
            }

            if (signer == null) return;
                
            File.WriteAllText(configName, config);
            signer.GenerateKeys();
            CloseKey = signer.SerializeKeys(true);
            OpenKey = signer.SerializeKeys(false);
        }
        
        /// <summary>
        /// MakeLicense and save into folder
        /// </summary>
        /// <param name="hardwareID">hardware id</param>
        /// <param name="expirationDate"></param>
        /// <param name="comment">comment</param>
        /// <param name="email">email</param>
        /// <returns>User license</returns>
        internal static string MakeLicense(string hardwareID, DateTime? expirationDate, string comment, string email)
        {
            UpdateKeys();
            if (CloseKey == null) return null;

            var path = BasePath;
            var signer = new SerialNumberSigner(CloseKey);

            var lic = signer.Sign(hardwareID + (expirationDate.HasValue ? 
                "DateExpiration:" + expirationDate.Value.ToString("yyyy.MM.dd") : null));

            var licIn = lic;
            var licOut = "";
            
            for (var i = 0; i < lic.Length; i += 4)
            {
                if (string.IsNullOrEmpty(licOut) == false) licOut += "-";
                
                licOut += licIn[..Math.Min(4, licIn.Length)];
                licIn = licIn.Remove(0, Math.Min(4, licIn.Length));
            }
            
            if (licIn.Length > 0)
            {
                licOut += licIn[..Math.Min(4, licIn.Length)];
                licIn.Remove(0, Math.Min(4, licIn.Length));
            }
            
            lic = licOut;
            if (expirationDate.HasValue)
            {
                lic += "-@" + (int)(expirationDate.Value.ToUniversalTime() - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalDays;
            }
            var file = Path.Combine(path, hardwareID + (expirationDate.HasValue ? 
                (string.IsNullOrEmpty(hardwareID) ? "" : "-") + "Expiration-" + expirationDate.Value.ToString("yyyy.MM.dd") : "") + ".lic");

            File.WriteAllText(file, lic);
            File.WriteAllText(file + ".comment", comment);
            File.WriteAllText(file + ".email", email);
            return lic;
    }
        
        /// <summary>
        /// Return all generated licenses
        /// </summary>
        /// <returns>License list</returns>
        internal static List<LicenseInfo> GetLicenseList()
        {
            if (LicenseList != null) return LicenseList;

            var path = BasePath;

            var list = new SortedList<long, LicenseInfo>();
            foreach (var file in Directory.GetFiles(path, "*.lic"))
            {
                var lic = new LicenseInfo
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    LicenseContent = File.ReadAllText(file)
                };

                var inf = new FileInfo(file);
                lic.CreateDate = inf.CreationTime;
                if (File.Exists(file + ".comment")) lic.Comment = File.ReadAllText(file + ".comment");
                if (File.Exists(file + ".email")) lic.Email = File.ReadAllText(file + ".email");

                list.Add(inf.LastWriteTimeUtc.ToFileTimeUtc(), lic);
            }
            
            LicenseList = new List<LicenseInfo>(list.Values);
            LicenseList.Reverse();
            return LicenseList;
        }


        internal class LicenseInfo
        {
            internal string Name;
            internal DateTime CreateDate;
            internal string Comment;
            internal string Email;
            internal string LicenseContent;
        }
    }
