using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration
{
    public class GenerateChaCha20KeyRequestDto
    {
        [Required]
        public GenerateKeyAttributesDto KeyAttributes
        {
            get;
            set;
        }

        public GenerateChaCha20KeyRequestDto()
        {
            this.KeyAttributes = default!;
        }
    }
}
