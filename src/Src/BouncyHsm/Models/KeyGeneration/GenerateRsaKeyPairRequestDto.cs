using BouncyHsm.Core.UseCases.Contracts;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration
{
    public class GenerateRsaKeyPairRequestDto
    {
        [Required]
        public int KeySize
        {
            get;
            set;
        }

        [Required]
        public GenerateKeyAttributesDto KeyAttributes
        {
            get;
            set;
        }

        public GenerateRsaKeyPairRequestDto()
        {
            this.KeyAttributes = default!;
        }
    }
}
