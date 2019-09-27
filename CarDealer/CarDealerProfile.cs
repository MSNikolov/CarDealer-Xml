using AutoMapper;
using CarDealer.Dtos.Export;
using CarDealer.Dtos.Import;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<ImportSupplier, Supplier>();

            this.CreateMap<ImportPart, Part>();

            this.CreateMap<ImportCar, Car>();

            this.CreateMap<ImportCustomer, Customer>();

            this.CreateMap<ImportSale, Sale>();

            this.CreateMap<Car, CarWithDistance>();
        }
    }
}
