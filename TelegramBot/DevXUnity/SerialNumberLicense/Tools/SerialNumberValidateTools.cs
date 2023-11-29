using System;
using System.Text;
//using UnityEngine;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace DevXUnity.SerialNumberLicense.Tools
{
    //internal static class SerialNumberValidateTools
    //{
    //    private const string HardwareIDKey = "_HardwareID";
        
    //    #region Static fields
    //    private static readonly string RegKey = (Application.productName + "_dxserialnumber_").GetHashCode().ToString();
    //    private static DateTime? _lastTime;
    //    private static bool _timeValid = true;
    //    #endregion

    //    #region Properties
    //    internal static string SerialNumberKey
    //    {
    //        set
    //        {
    //            PlayerPrefs.SetString("SerialNumberKey", value);
    //            PlayerPrefs.Save();
    //        }
    //        get => PlayerPrefs.GetString("SerialNumberKey");
    //    }
        
    //    internal static string HardwareID
    //    {
    //        get
    //        {
    //            var id = PlayerPrefs.GetString(GetStringHashAsHex(HardwareIDKey), "");
    //            if (!string.IsNullOrEmpty(id)) return GetStringHashAsHex(id);
                
    //            var nets = NetworkInterface.GetAllNetworkInterfaces();
    //            var address = nets[0].GetPhysicalAddress();
    //            foreach (var net in nets)
    //                if (net.OperationalStatus == OperationalStatus.Up && 
    //                    net.NetworkInterfaceType != NetworkInterfaceType.Tunnel && 
    //                    net.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
    //                    net.GetIPv4Statistics().BytesReceived > 0)
    //                    address = net.GetPhysicalAddress();

    //            id = GetStringHashAsHex(address + Application.productName+Application.companyName);
    //            PlayerPrefs.SetString(GetStringHashAsHex(HardwareIDKey), id);
    //            PlayerPrefs.Save();

    //            return GetStringHashAsHex(id);
    //        }
    //    }
    //    #endregion

    //    internal static bool Verify(out int? licenseExpiredAfterNumDays)
    //    {
    //        licenseExpiredAfterNumDays = null;

    //        var now = DateTime.UtcNow;
    //        var timeValid = UpdateTime();
            
    //        var licFile = Resources.Load<TextAsset>("SN-License-OpenKey");
    //        if (licFile == null || string.IsNullOrEmpty(licFile.text)) return false;
            
    //        var openKey = licFile.text;
    //        var signer = new SerialNumberVerify(openKey);

    //        if (string.IsNullOrEmpty(SerialNumberKey)) return false;
            
    //        DateTime? expirDate = null;

    //        var hardwareID = HardwareID;
    //        var serNum = SerialNumberKey.Trim();

    //        if (!SerialNumberKey.Contains("-@")) return signer.VerifySignature(hardwareID, serNum);
            
    //        serNum = SerialNumberKey[..SerialNumberKey.IndexOf("-@", StringComparison.Ordinal)].Trim();
    //        var stick = SerialNumberKey[(SerialNumberKey.IndexOf("-@", StringComparison.Ordinal) + "-@".Length)..].Trim();
                
    //        try
    //        {
    //            expirDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(int.Parse(stick)).AddHours(23);
    //        }
    //        catch
    //        {
    //            // ignored
    //        }

    //        var res = signer.VerifySignature(hardwareID, serNum);
    //        if (res || !expirDate.HasValue) return res;
                
    //        if (expirDate.Value < now)
    //        {
    //            licenseExpiredAfterNumDays = 0;
    //            return false;
    //        }
                
    //        licenseExpiredAfterNumDays = (int)(expirDate.Value - now).TotalDays;

    //        if (!timeValid) return false;
                
    //        res = signer.VerifySignature("DateExpiration:" + expirDate.Value.ToString("yyyy.MM.dd"), serNum);
    //        if (res == false) res = signer.VerifySignature(hardwareID + "DateExpiration:" + expirDate.Value.ToString("yyyy.MM.dd"), serNum);

    //        return res;
    //    }

    //    private static bool UpdateTime()
    //    {
    //        if (_lastTime.HasValue && (DateTime.UtcNow - _lastTime.Value).TotalMinutes < 1)
    //            return _timeValid;
            
    //        var now = DateTime.UtcNow;
    //        _lastTime = now;

    //        _timeValid = true;

    //        var stiks = PlayerPrefs.GetString(RegKey);
    //        if (string.IsNullOrEmpty(stiks) == false)
    //        {
    //            var timeNow = new DateTime(long.Parse(stiks), DateTimeKind.Utc);
    //            if (now < timeNow) _timeValid = false;
    //        }

    //        if (_timeValid)
    //        {
    //            PlayerPrefs.SetString(RegKey, now.Ticks.ToString());
    //            PlayerPrefs.Save();
    //        }
    //        return _timeValid;
    //    }

    //    #region Get string hash functions
    //    private static uint GetStringHash(string str)
    //    {
    //        var encoding = Encoding.Unicode;
    //        MD5 md5 = new MD5CryptoServiceProvider();
    //        var result = md5.ComputeHash(encoding.GetBytes(str));
    //        return BitConverter.ToUInt32(result, 0);
    //    }

    //    private static string GetStringHashAsHex(string s) => $"{GetStringHash(s):X}";
    //    #endregion
    //}
}
