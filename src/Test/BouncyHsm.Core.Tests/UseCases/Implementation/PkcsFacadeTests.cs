using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class PkcsFacadeTests
{
    [TestMethod]
    [DataRow(PrivateKeyImportMode.Local, true, 0)]
    [DataRow(PrivateKeyImportMode.Local, false, 0)]
    [DataRow(PrivateKeyImportMode.Imported, false, 0)]
    [DataRow(PrivateKeyImportMode.LocalInQualifiedArea, false, 0)]
    [DataRow(PrivateKeyImportMode.Imported, true, 1)]
    public async Task ImportP12_Call_Success(PrivateKeyImportMode mode, bool withChain, int certId)
    {

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

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, TimeProvider.System, new NullLogger<PkcsFacade>());

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

    [TestMethod]
    public async Task ImportPem_MlDsaPublic_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           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
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_MlDsaPrivate_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           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
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_SlhDsaPublic_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MFAwCwYJYIZIAWUDBAMYA0EAgwXsNWDW4SX2vBl8wo0e1Aq2+gF9eU/YnNOG4xMH
           7S5mSyRVy5Lq8+6Nj61HA8W78+cmw1PtK9k4YjZ2Ph+45Q==
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_SlhDsaPrivate_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           MIGTAgEAMAsGCWCGSAFlAwQDGASBgO+xWzcKLSBhDwwNMg5PmzshVtRkDv/IqxxG
           1BoLFEJ96LL6MynFQ+zyHFQcSd9qr3nO0fo554xcI7icWzVsVg6DBew1YNbhJfa8
           GXzCjR7UCrb6AX15T9ic04bjEwftLmZLJFXLkurz7o2PrUcDxbvz5ybDU+0r2Thi
           NnY+H7jl
           -----END PRIVATE KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_MLKemPublic_Success()
    {
        string pem = """
           -----BEGIN PUBLIC KEY-----
           MIIDMjALBglghkgBZQMEBAEDggMhAONKd48XBMmLBXu7KaSKE0xVPwvEdtp6NgEz
           n/6FAUBQp4H6IQeYICbjVn/mEgw7C+JGCWkFXuLhiOPDL8pZZI96FJqWaY43bf+h
           sbH4FMK4ZWVhO/HcVfuHJS1cmWkGl7FAhz/8b6bBBL1GzGepxzHCBOcpJkRkJ7/2
           x61lnnyYY693Sx2IlBwkXGQHx7pnQz9JRU/ztDwZLSsQJJzbmKtEF9X4hYM3naYA
           viJcjcDQTB1pvKpVndPMHGVaKbFzo+0pb51Cs1w6Wa5bwPqYrTf4rBlnH02oDuWi
           eyykaxMQqS1XJ3RLBpT5euUjafYhx4VwQqWSKqlrwzashdvrGEgSyHDya+RpEveD
           CyNbG+I1pPSya4+3Z5WUQgORzLH1ebVHUNxXfqYmtWmKU7LiJgsbrixDimnAcDu7
           WJShDD3EFGmYxah1fpdSgQcabS2MxXHivYhmExkwX22SFYxWOex1QQF0l4KwCnvm
           vYzZJgE7F2oTbccDglIFoA1DYkBzLsqKGbvUByT1L5Uiwr1QzeM6LkRVMc08waYp
           GwjBG6sKUHDaYEJlA2u3kg/VEZC0T+5ULMqpCU5jQps7DvlcxXVFldI3ALLztjpm
           VSuUG7C6nbbDnW5CjM00gznXrAR7tczwPSABWXP1E1JAn/U4AUvQdyYRqWxZABBA
           pavosGEId9UACZvFsFy1G+iseekbSy1rm4c5zTgakXF8hHiJwCclKS0gQm6YinYo
           SEobd0ccoJQoC8MXhZfhz066fg9px+98D05WQfNqqqe4WsW8h9cUPm/KBVkHleFr
           mtkibm4AjMdbHMc0TP+RGojxo5HrreLSigd8MMzaCddSNPCoSaDHsrxlM8R7gH1C
           Yh+jmAk4D+14dFr0DGYEyojDL7zgA5FcoLtoh/SMh9V8v4Bmx2ECzL06iBw2j01T
           Ta0Sa6LJWD/bkRrcl5aQZhWFm/qlDm7jV0h6AxDxV5cmbJF6eqxEbhfZU97yqzO1
           fjtwilkERwbAfhxHEjCDcolGzrjipZW3JNTMaiYSfBq01Cd9bm4rrchMJqC3u486
           kR0ft09K
           -----END PUBLIC KEY-----
           """;
        await this.ImportPemTest(pem);
    }

    [TestMethod]
    public async Task ImportPem_MLKemPrivate_Success()
    {
        string pem = """
           -----BEGIN PRIVATE KEY-----
           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
           xmXQQTFDRX1RhWkUpvls1BcPGak0TjOs+mL9Vg==
           -----END PRIVATE KEY-----
           
           """;
        await this.ImportPemTest(pem);
    }

    private async Task ImportPemTest(string pem)
    {
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

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, TimeProvider.System, new NullLogger<PkcsFacade>());

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

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, TimeProvider.System, new NullLogger<PkcsFacade>());

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