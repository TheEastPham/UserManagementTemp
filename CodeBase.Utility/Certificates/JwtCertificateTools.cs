using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace CodeBase.Utility.Certificates;

public static class JwtCertificateTools
{
    public static X509Certificate2 GetIdentityCertificate(string basePath, IConfiguration configuration)
    {
        var path =  configuration["Certificates:Path"] ?? "IS4.pfx" ;
        var pfxPath = Path.Combine(basePath,path);
        var pfxPassword =  configuration["Certificates:Password"] ?? "xB123456" ;
        return new X509Certificate2(pfxPath, pfxPassword);
    }
    
    static X509Certificate2 GenerateCertificate(string certName)
    {
        var keypairgen = new RsaKeyPairGenerator();
        keypairgen.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 1024));
        var keypair = keypairgen.GenerateKeyPair();
        var cn = new X509Name("CN=" + certName);
        var sn = BigInteger.ProbablePrime(120, new Random());
        var gen = new X509V3CertificateGenerator();
        gen.SetSerialNumber(sn);
        gen.SetSubjectDN(cn);
        gen.SetIssuerDN(cn);
        gen.SetNotAfter(DateTime.MaxValue);
        gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
        gen.SetSignatureAlgorithm("MD5WithRSA");
        gen.SetPublicKey(keypair.Public);         
        var newCert = gen.Generate(keypair.Private);
        return new X509Certificate2(DotNetUtilities.ToX509Certificate((Org.BouncyCastle.X509.X509Certificate)newCert));
    }
}