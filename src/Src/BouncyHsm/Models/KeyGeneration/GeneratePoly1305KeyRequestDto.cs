using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration
{
    public class GeneratePoly1305KeyRequestDto
    {
        [Required]
        public GenerateKeyAttributesDto KeyAttributes
        {
            get;
            set;
        }

        public GeneratePoly1305KeyRequestDto()
        {
            this.KeyAttributes = default!;
        }
    }
}
