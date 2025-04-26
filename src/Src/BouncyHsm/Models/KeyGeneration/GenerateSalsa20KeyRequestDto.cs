using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration
{
    public class GenerateSalsa20KeyRequestDto
    {
        [Required]
        public GenerateKeyAttributesDto KeyAttributes
        {
            get;
            set;
        }

        public GenerateSalsa20KeyRequestDto()
        {
            this.KeyAttributes = default!;
        }
    }
}
