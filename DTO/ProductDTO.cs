namespace CartWebApi.DTO
{
    public class ProductDTO
    { 
        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }
    
        public int Quantity { get; set; }
       
        public int CategoryId { get; set; }

        public IFormFile Image { get; set; }
    }

}
