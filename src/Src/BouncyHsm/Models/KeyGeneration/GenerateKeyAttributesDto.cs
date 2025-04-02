using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration
{
    public class GenerateKeyAttributesDto
    {
        [Required]
        [MaxLength(1024)]
        public string CkaLabel
        {
            get;
            set;
        }

        [MaxLength(1024)]
        public byte[]? CkaId
        {
            get;
            set;
        }

        [Required]
        public bool Exportable
        {
            get;
            set;
        }

        [Required]
        public bool Sensitive
        {
            get;
            set;
        }

        [Required]
        public bool ForSigning
        {
            get;
            set;
        }

        [Required]
        public bool ForEncryption
        {
            get;
            set;
        }

        [Required]
        public bool ForDerivation
        {
            get;
            set;
        }

        [Required]
        public bool ForWrap
        {
            get;
            set;
        }

        public GenerateKeyAttributesDto()
        {
            this.CkaLabel = string.Empty;
        }
    }
}
