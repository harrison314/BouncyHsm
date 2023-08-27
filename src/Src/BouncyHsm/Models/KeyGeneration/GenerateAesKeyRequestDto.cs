using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration
{
    public class GenerateAesKeyRequestDto
    {
        [Required]
        public int Size
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

        public GenerateAesKeyRequestDto()
        {
            this.KeyAttributes = default!;
        }
    }
}
