using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class PkcsFacadeTests
{
    [DataTestMethod]
    [DataRow(PrivateKeyImportMode.Local, true, 0)]
    [DataRow(PrivateKeyImportMode.Local, false, 0)]
    [DataRow(PrivateKeyImportMode.Imported, false, 0)]
    [DataRow(PrivateKeyImportMode.LocalInQualifiedArea, false, 0)]
    [DataRow(PrivateKeyImportMode.Imported, true, 1)]
    public async Task ImportP12_Call_Success(PrivateKeyImportMode mode, bool withChain, int certId)
    {
        Mock<ITimeAccessor> timeAccessor = new Mock<ITimeAccessor>(MockBehavior.Strict);

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is X509CertificateObject || q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlotEntity()
            {
                Description = "description",
                Id = Guid.NewGuid(),
                IsHwDevice = true,
                SlotId = 12,
                Token = new TokenInfo()
                {
                    IsSoPinLocked = false,
                    IsUserPinLocked = false,
                    Label = "Label",
                    SerialNumber = "000011",
                    SimulateHwMechanism = true,
                    SimulateHwRng = true,
                    SimulateQualifiedArea = true
                }
            })
            .Verifiable();

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, timeAccessor.Object, new NullLogger<PkcsFacade>());

        ImportP12Request request = new ImportP12Request()
        {
            SlotId = 12U,
            CkaId = new byte[8],
            CkaLabel = "Test",
            ImportChain = withChain,
            ImportMode = mode,
            Password = "Passw0rd",
            Pkcs12Content = this.GetP12Content(certId)
        };

        DomainResult<Guid> domainResult = await pkcsFacade.ImportP12(request, default);
        _ = domainResult.AssertOkValue();

        repository.VerifyAll();
    }

    [TestMethod]
    [Ignore]
    public async Task GetObjects_Call_Success()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public async Task GeneratePkcs10_Call_Success()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public async Task ImportX509Certificate_Call_Success()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public async Task DeteleAsociatedObjects_Call_Success()
    {
        throw new NotImplementedException();
    }

    [TestMethod]
    public async Task ImportPem_Certificate_Success()
    {
        string pem = """
           -----BEGIN CERTIFICATE-----
           MIICMzCCAZygAwIBAgIJALiPnVsvq8dsMA0GCSqGSIb3DQEBBQUAMFMxCzAJBgNV
           BAYTAlVTMQwwCgYDVQQIEwNmb28xDDAKBgNVBAcTA2ZvbzEMMAoGA1UEChMDZm9v
           MQwwCgYDVQQLEwNmb28xDDAKBgNVBAMTA2ZvbzAeFw0xMzAzMTkxNTQwMTlaFw0x
           ODAzMTgxNTQwMTlaMFMxCzAJBgNVBAYTAlVTMQwwCgYDVQQIEwNmb28xDDAKBgNV
           BAcTA2ZvbzEMMAoGA1UEChMDZm9vMQwwCgYDVQQLEwNmb28xDDAKBgNVBAMTA2Zv
           bzCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAzdGfxi9CNbMf1UUcvDQh7MYB
           OveIHyc0E0KIbhjK5FkCBU4CiZrbfHagaW7ZEcN0tt3EvpbOMxxc/ZQU2WN/s/wP
           xph0pSfsfFsTKM4RhTWD2v4fgk+xZiKd1p0+L4hTtpwnEw0uXRVd0ki6muwV5y/P
           +5FHUeldq+pgTcgzuK8CAwEAAaMPMA0wCwYDVR0PBAQDAgLkMA0GCSqGSIb3DQEB
           BQUAA4GBAJiDAAtY0mQQeuxWdzLRzXmjvdSuL9GoyT3BF/jSnpxz5/58dba8pWen
           v3pj4P3w5DoOso0rzkZy2jEsEitlVM2mLSbQpMM+MUVQCQoiG6W9xuCFuxSrwPIS
           pAqEAuV4DNoxQKKWmhVv+J0ptMWD25Pnpxeq5sXzghfJnslJlQND
           -----END CERTIFICATE----- 
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_RsaPrivateKey_Success()
    {
        string pem = """
           -----BEGIN RSA PRIVATE KEY-----
           MIIEpQIBAAKCAQEAuN9DJTv4kmgu064D8Zyg7OKVHlMeiCs5teCZqUstMXUt77KW
           49EiEGIHz+sSYAsy5N/jdLy0sFofQkG6Xo8CxDeGg799wJcfJwG7Ayh0x3ztN98R
           MNtEy3b9WVzyT0w5Shwfim6zAO6SzgEhVtNLobfbhuOPhxAJnhoKWNoiog/N7ejN
           Cl9UFI1kY8M8M/LIjuokWvEM0+L/L+BM5vITq79Ws4F1/qjJg0rELUMw4oQeQMas
           ZuglPvYkQqCrETALxiawJSb1TTCp/Ey9OZM1CLittLOvTW5DdBw7KM8V0P8zvuIS
           itUjnNgz1lQbfxqdWu81902beHB6o9QjpzgFtwIDAQABAoIBAQCahPmBUJvV+0BQ
           a10egDS9ajELFJwrYj2tOBoXNx+B/Bg2BYY6ylz3ZohzD17fadzTEhLySpuX3uvL
           nFZinJPKX0KOMeqwo19FYhvmatUYu+EmVsrulAbvLPhazeY1w1cLC3CNazMwrzeC
           +czc3mSTubHCD8eyMwRm4gsN8t0JqFJH/ucDJsHHfcu5BPyyL94TWmKCEietTfiI
           8Ki8GH8OwVw4I9wy6+6F6+7joMR/B1OvV8x1rtFuk8wH5PSanEf1BPylDzFD/HAp
           W4bDqlwrOSy2aMXto6OK8G3cgiyftSqNzOIhwarAVghB6bZUBU7kFrLGvn8H77GM
           VTHzQXLJAoGBAN7d05pQe+P2gZLHprLLw+hRiS0kFjzFiibS5283Xkf7JkrMICMa
           dh9Zlli5JYvmc5FerKgm0x92JpWLkDkpo/qsBklfxcVsPgLqTq4xSZtH/rbw5mHl
           3i301/XMt8y4dJpLed5aYQmhU93FZcjGCIC8fdsVjGOpQ30bqBJUErQbAoGBANRb
           ZACooo0uQEvkhO9Bb+zR3OXmyj3fJXrDSbzVE4yx0zQd7ceHYITZtNiNvf8ECdhe
           /AcYH1iG23s8xxP+HbCzUvAA1105i3AUBt9TDuNlIQYs4Of1sfYWq1m3v1M4HdPJ
           F/EL/xy5zwGq8jN1YHtmc5qEzWchjVKlPhLFfraVAoGAOeKn2UbaRuV51iPhGkNu
           iOLUnFLpK7OrJFZXIj3hURTcZ0UJe9SdpZrhP/4m0GV00uciNTKQV3Wao/Dx7sbv
           /mW75Ebp2VM58Avnj7rhgWF7uQxs6jSINquHhCI+AwBN2N2Ns8EJvzSV0d45h6JY
           BwfuMH8yTZhjHRWX29rWWM0CgYEAtbHEbLPc8UMjjEvoWfX5V/1wLd08KZgmL1Ws
           X79ITNdRyIPbER+Ju+GyVJ9iczH3YoRSy5ceKtaoMFeeVkLVEH0+d0+g9Yjo/2qD
           Ps2ILZQ3n1sCzDVyoQZgchE/yGp5St4CeCI1k1SABANJ3DGP7cWJICqEvLr+ejoc
           VF9avckCgYEAvF04CIssbqHzgioGu6886fgCyuNkmZraP5DsfS/0AWyEIzIB1g8k
           K653XtGuxn544mROGPzjzeHtADX0qa5AnQcr+Ort6e0iO4Ht3kZPkVM7iph1qGsT
           fScgKkkyyvumEZGIfgAiwPWuBbEbQSsXMkP5Xb3L31i1e5LyK8f0tNA=
           -----END RSA PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_RsaPkcs8_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCjfdKE8hHXYMfi
           OETUIit+swiYCwDVbACa6hCPgPZQf919pAXzqV28DW6EHCkvP5Zy2S2GIWVXKL9c
           zms3iEOGyLoGEcQ9Y0PYhcUswxKijz/53KXRi3Qngd1YbuxyZiT99Ic9R7jEW+3Q
           w5De2CRGq351oz9eXhCuUMq/+ChObaKbYNkpVBvD+udzO+8Vf79Z1Z4qBFR8b5WD
           +zPTiU2VDnFlTniRWCGjJfz46HZsGK1+2amsbhKNa42FNUZ3hB/VGcAkrZfn8Ebk
           katZq/1HHFL6mBqtYmYgQZx5s4+e57aLYBzj+rxvAAchl/6nJd57WivZksymUYdo
           Ub5pkL8xAgMBAAECggEADUWBeVlNKXapwyteKvo7HaXa5Ly/7JM/2VN21K9bT12R
           1UkjUoxFF6bfecnvbe5zgA8xKto7J8AfCKGZAoEFOkPBFg0LKRCNyV3Si7eqI5gN
           UXMf4sq3Ox3Hog4fE4pHJnZbJBZWYVo1C+VUNULGbxYsxc/irP5lzECytLKoUvVi
           I4/IpPCSRioY6GAfpMMl/Bqwfp2M+Ly8CVUS88g10qMq+vt2z/5vHNfPfdipj6u3
           OpHtE/q1OiEs5gyploiJNtBJQIG5ekdx5RL6Oz1Zs5jXl08Bjt8zB5DddwC0KxHT
           70cZPjlTnb/31heGC1Cx7ujy9slEoNtcsFTIeNXCoQKBgQDXZiIzUDxwagPZKqkH
           qXO5dM4awCkExo8hB5ibAaHMCtKYWdETvygMSWR3cNc1P+rPwnJDL4qzJivJquyk
           oDF99SXaL+f2UNPKMJXZGiVXesBzdhyqVj9gLsJ1IokBPHv7UlnbbWK1zq0C1ySn
           53oZwBSBW0WWYxKoy+VME/degwKBgQDCTvHWgs1o0FK4s+3BE18K72QRZt79BqME
           K31WkLGIt5yWiQ/6Z6+MiADU8k9DFNPWgRBqcDHY0ZNZQWDm+wWm6R6gPWL8P64v
           4MoxzftwPi9y82s6jDmk/gNWsn2OM+/SRG1mjGm96a376j2hS3kV+kRrwdnxoxoI
           Al4qE3h9OwKBgQDWTRwLt3FaWm+XuZTQNawYQHjqLnLg+HfgYcFXvqjt63qY7wtP
           vSioCMD3AIJszTneGFQ8Oemh0YFRNEgahfKXobZWPMFo1APSrsH3bMboIQ2mEkX0
           xrhpBjyb848hdr7XTZhu8oZ54bVKFSi4EFnvkqYUCO3T8J/Y5nssVNUQ5wKBgQCl
           RHxR1cNciQQy8Wcht5Y5ONBGNNcpI0H4Q/1BaaR3AqT/LOkYNKSNxQfgF5DvH4Hm
           irQps+/R2L+ZRRBkpdFy3AkehdfxcUB4nJudrPNVzq6Q+RWVILvO5/ZzATHlh6tN
           jsH2XSt7SoyfHeb5j7YXyVv0w1baPb3gXhM1eoYbDQKBgF5wlWK0sttj485qWmm7
           yX5DDJRJMJMEupLytEGHPzXYfWZTx/uCeHCf4douC3Xi4Eku7p+N1O7I+AGz3XbE
           5T9qLDHXjtfrR5AvVeGYv/ZIWko0CzNn8o86LUpMPJU6PIkCBi6V4XHNChDZtsnO
           1pbtAWtNwz//4idQhQ5bxr68
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_EcPkcs8_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQgdFZuDHw/XzWjO28BAQvK
           4cTOA5mbWBUWrClI2K7pw6OhRANCAAR8SEgi/OV/c/kxmIlNjSp1+ADlxnUEbj4x
           JtSL9LCVCvO8DI48SuQPOfXrkjgb1rgbGgFFFyeKrv+APnAggb2Y
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_EcKeyAndCert_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQghrSAsNdEiCXTR1h/VkIw
           OeNUwqi1291TiIpr/QC2gXahRANCAATC1LhaWM2sj7C5gliIcP1lYhEjXbeYgKO+
           MIlBj0eVZY4LMAKwVodiyeYlOtAMBndJ6goyQWfY+PCUgyYZSAsy
           -----END PRIVATE KEY-----
           -----BEGIN CERTIFICATE-----
           MIIB+jCCAZ+gAwIBAgIUDCXazphao8sG6ot7FW+60TFT7mQwCgYIKoZIzj0EAwIw
           QzELMAkGA1UEBhMCU0sxIDAeBgNVBAgMF0JhbnNrw6EgQnlzdHJpY2EgUmVnaW9u
           MRIwEAYDVQQHDAlOb3ZhIEJhbmEwHhcNMjQwMzE2MTQyNjU3WhcNMjUwMzE2MTQy
           NjU3WjBDMQswCQYDVQQGEwJTSzEgMB4GA1UECAwXQmFuc2vDoSBCeXN0cmljYSBS
           ZWdpb24xEjAQBgNVBAcMCU5vdmEgQmFuYTBWMBAGByqGSM49AgEGBSuBBAAKA0IA
           BMLUuFpYzayPsLmCWIhw/WViESNdt5iAo74wiUGPR5VljgswArBWh2LJ5iU60AwG
           d0nqCjJBZ9j48JSDJhlICzKjdDByMB0GA1UdDgQWBBQXLi7f+Gp8WFZM78wKvMHp
           E/oT3jAfBgNVHSMEGDAWgBQXLi7f+Gp8WFZM78wKvMHpE/oT3jAOBgNVHQ8BAf8E
           BAMCBaAwIAYDVR0lAQH/BBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMAoGCCqGSM49
           BAMCA0kAMEYCIQDZ39a7NIpLKM8YbjLL4YTQIC3koJbXGRtHuasLJXmMXgIhAJWQ
           PQr1eYzspGAaHmU/+ItvHY6otaHKqBhX3whUfm/J
           -----END CERTIFICATE-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_DataObject_Success()
    {
        string pem = """
           -----BEGIN DATA OBJECT-----
           SGVsbG8gd29ybGQh
           -----END DATA OBJECT-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_DataObjectWithApp_Success()
    {
        string pem = """
           -----BEGIN DATA OBJECT-----
           Application: MyApplication
           SGVsbG8gd29ybGQh
           -----END DATA OBJECT-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_DataObjectWitObjectId_Success()
    {
        string pem = """
           -----BEGIN DATA OBJECT-----
           ObjectId: 2.5.4.97
           SGVsbG8gd29ybGQh
           -----END DATA OBJECT-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_AesKey_Success()
    {
        string pem = """
           -----BEGIN AES SECRET KEY-----
           W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
           -----END AES SECRET KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_Poly1305Key_Success()
    {
        string pem = """
           -----BEGIN POLY1305 SECRET KEY-----
           W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
           -----END POLY1305 SECRET KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_ChaCha20Key_Success()
    {
        string pem = """
           -----BEGIN CHACHA20 SECRET KEY-----
           W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
           -----END CHACHA20 SECRET KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_Salsa20Key_Success()
    {
        string pem = """
           -----BEGIN SALSA20 SECRET KEY-----
           W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
           -----END SALSA20 SECRET KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_GenericSecret_Success()
    {
        string pem = """
           -----BEGIN GENERIC SECRET-----
           W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
           -----END GENERIC SECRET-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_GenericSecretWithType_Success()
    {
        string pem = """
           -----BEGIN GENERIC SECRET-----
           KeyType: CKK_SHA256_HMAC
           W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
           -----END GENERIC SECRET-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_RsaPublicKeyPkcs8_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAKj34GkxFhD90vcNLYLInFEX6Ppy1tPf
           9Cnzj4p4WGeKLs1Pt8QuKUpRKfFLfRYC9AIKjbJTWit+CqvjWYzvQwECAwEAAQ==
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_RsaPublicKey_Success()
    {
        string pem = """
           -----BEGIN RSA PUBLIC KEY-----
           MEgCQQCo9+BpMRYQ/dL3DS2CyJxRF+j6ctbT3/Qp84+KeFhnii7NT7fELilKUSnx
           S30WAvQCCo2yU1orfgqr41mM70MBAgMBAAE=
           -----END RSA PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_Ed25519Private_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MFECAQEwBQYDK2VwBCIEIF81FEQ2hzC0Op/itfBNfKb6BsS2fJaan0JUQljZyNFw
           gSEA30lwQHhDzxQOXx7rB/+8fiQIiT4Eh1XBtic/kvGzWKQ=
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_Ed25519Public_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MCowBQYDK2VwAyEA30lwQHhDzxQOXx7rB/+8fiQIiT4Eh1XBtic/kvGzWKQ=
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }


    [TestMethod]
    public async Task ImportPem_Ed448Private_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MIGDAgEBMAUGAytlcQQ7BDlbI3ZuBrhiPuDiNleGVMhqwDi8fnm4NtHQ4R0UpSyk
           I9B5AZ/xob+dYNjSfqlwSHrl4IoWEcYNXXqBOgCHJ7H+QtuP8D3BzsVcbrOXS/RC
           Y7e6JrvD0VFOnasANe4dAf9ScwjRwxjZ3MSrjsrZqnbhIu6wxQA=
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_Ed448Public_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MEMwBQYDK2VxAzoAhyex/kLbj/A9wc7FXG6zl0v0QmO3uia7w9FRTp2rADXuHQH/
           UnMI0cMY2dzEq47K2ap24SLusMUA
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_X25519Private_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MFECAQEwBQYDK2VwBCIEIFj6oaYEHeFMwR9ealoNH736bSCuYuRDvqQaGeiV4f1W
           gSEA0icnnt8HyWsgvna0w6SB5y9qlVv2FQT7Gi/e8J0sCxE=
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_X25519Public_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MCowBQYDK2VuAyEArSuB8hXcp5AjM3xoLj1Uwi27NphJ15kwo4BJiGmH9E8=
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_X448Private_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MIGBAgEBMAUGAytlbwQ6BDhAdD0Zvo1fVgKjEGIizg98ZOj26iL5I67zqEyYOWJg
           bQ9o7fWMvUeeArj7Uf4MJCr4n1+FMUZHjIE5ACqRY55NO+eQN/RvAVtW0SeeLU2f
           nU4ykswVJu9MAdFHYxBr1jxMDGJOx9I4/5gslLyeqPdBdx00
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_X448Public_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MEIwBQYDK2VvAzkAKpFjnk0755A39G8BW1bRJ54tTZ+dTjKSzBUm70wB0UdjEGvW
           PEwMYk7H0jj/mCyUvJ6o90F3HTQ=
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    private async Task ImportPemTest(string pem)
    {
        Mock<ITimeAccessor> timeAccessor = new Mock<ITimeAccessor>(MockBehavior.Strict);

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlotEntity()
            {
                Description = "description",
                Id = Guid.NewGuid(),
                IsHwDevice = true,
                SlotId = 12,
                Token = new TokenInfo()
                {
                    IsSoPinLocked = false,
                    IsUserPinLocked = false,
                    Label = "Label",
                    SerialNumber = "000011",
                    SimulateHwMechanism = true,
                    SimulateHwRng = true,
                    SimulateQualifiedArea = true
                }
            })
            .Verifiable();

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, timeAccessor.Object, new NullLogger<PkcsFacade>());

        ImportPemRequest request = new ImportPemRequest()
        {
            SlotId = 12U,
            CkaLabel = "label1",
            Pem = pem,
            Hints = new ImportPemHints()
            {
                ForDerivation = true,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = true,
                ImportMode = PrivateKeyImportMode.Local
            }

        };

        DomainResult<IReadOnlyList<Guid>> domainResult = await pkcsFacade.ImportPem(request, default);
        _ = domainResult.AssertOkValue();

        repository.VerifyAll();
    }

    [TestMethod]
    public async Task ParseCertificate_Call_Success()
    {
        uint slotId = 12;
        Guid objectId = Guid.NewGuid();

        Mock<ITimeAccessor> timeAccessor = new Mock<ITimeAccessor>(MockBehavior.Strict);

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(slotId, objectId, It.IsAny<CancellationToken>()))
         .ReturnsAsync(new X509CertificateObject()
         {
             CkaValue = Convert.FromBase64String(@"MIIDnjCCAoagAwIBAgIBBDANBgkqhkiG9w0BAQsFADAiMQswCQYDVQQGEwJTSzET
MBEGA1UEAxMKT2NzcFRlc3RDYTAgFw0yMzAzMDExMTUyMDBaGA8yMTc1MDMwMTEx
NTIwMFowgYwxCzAJBgNVBAYTAlNLMRMwEQYDVQQHEwpCcmF0aXNsYXZhMRswGQYD
VQQDExJUZXN0IGNlcnRpZmljYXRlIDExEjAQBgNVBAUTCTEyMzQ1Njc4OTEZMBcG
A1UEDRMQU29tZSBzZWRjcmlwdGlvbjEcMBoGA1UEDRMTQW5vdGhlciBkZXNjcmlw
dGlvbjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAL92pQ7csbEY6/Cd
OWCr2txhrdXfhMw/ljNGge18AI44lf5T1oTpkGUMs+nS4vCNe4AqOifhz0VVWnuJ
FcdcT6PEMC5xGd8nMWneflECjOWX/lAqprMafAWtifu/CLV3KxVmuTkXNY+u2S6a
BBk94zniVLj4TWg7u4bGazc7A37USffEof7h5FIEVmnK7zNtiEjKoO4rmRQFgGwJ
Hl3AH6H5Peb3+/xU/Y4ecDABJXh7lLkm1rRFIwIES8qoiADrlDMGOdovf392bqVD
/IzsGMjNq/Up6QN2q/RteFrV+l0ymIe719w6kuz5K4ayUgXBIifkAh+k6MXwAibf
8bCm3fUCAwEAAaNyMHAwCQYDVR0TBAIwADAdBgNVHQ4EFgQUNek7YdB0yksh4q6X
m1q1tg6xwdAwHwYDVR0jBBgwFoAU1mB5PJXj9osKMG3/uvfTDrQW1QAwDgYDVR0P
AQH/BAQDAgbAMBMGA1UdJQQMMAoGCCsGAQUFBwMDMA0GCSqGSIb3DQEBCwUAA4IB
AQAcz8FGd4lVYwCJiPx2diFJ55MozExzzNpmE4L1AyaUiA0RcOddrqNCfLUyONaY
0dyKvPldenrueXo5x5lyYshKUaPTXGY+LupzmSIhIUx/nqHfX/5MDR9XBt3P32Yd
VKduDr/FqqXqlNd5WiRiquAeY9cNpMbrnOtnBDHUatXO/6z+t5mNhyZSbwYsqP+Q
tDB36ZXqy3/dFnjStOXbur6YTKusZmiT23B64ZphseAbivZzdKFXbRd4MzyzSdO3
HJEaJ5pTZXquh9ztiNgUD171gJ/YqA2tGsnIehfVi7XIzf51zoO/0iSHvF4eyctw
pU0+bapXOCAQP9suslVRcEn3")
         })
         .Verifiable();

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, timeAccessor.Object, new NullLogger<PkcsFacade>());

        DomainResult<CertificateDetail> domianResult = await pkcsFacade.ParseCertificate(slotId, objectId, default);
        CertificateDetail result = domianResult.AssertOkValue();

        Assert.That.IsNotNullOrEmpty(result.Issuer);
        Assert.That.IsNotNullOrEmpty(result.Subject);
        Assert.That.IsNotNullOrEmpty(result.SerialNumber);
        Assert.That.IsNotNullOrEmpty(result.SignatureAlgorithm);
        Assert.That.IsNotNullOrEmpty(result.Thumbprint);
    }

    private byte[] GetP12Content(int id)
    {
        string[] contents = new string[]
        {
            "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"
        };

        return Convert.FromBase64String(contents[id]);
    }
}