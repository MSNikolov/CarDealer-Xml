using AutoMapper;
using CarDealer.Data;
using CarDealer.Dtos.Export;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            Mapper.Initialize(cfg => cfg.AddProfile(new CarDealerProfile()));

            //var supplierReader = new StreamReader(@"E:\Кодене\CarDealer - Skeleton\CarDealer\Datasets\suppliers.xml");

            //var suppliers = supplierReader.ReadToEnd();

            //Console.WriteLine(ImportSuppliers(context, suppliers));

            //var partsReader = new StreamReader(@"E:\Кодене\CarDealer - Skeleton\CarDealer\Datasets\parts.xml");

            //var parts = partsReader.ReadToEnd();

            //Console.WriteLine(ImportParts(context, parts));

            //var carsReader = new StreamReader(@"E:\Кодене\CarDealer - Skeleton\CarDealer\Datasets\cars.xml");

            //var cars = carsReader.ReadToEnd();

            //Console.WriteLine(ImportCars(context, cars));

            //var customerReader = new StreamReader(@"E:\Кодене\CarDealer - Skeleton\CarDealer\Datasets\customers.xml");

            //var customers = customerReader.ReadToEnd();

            //Console.WriteLine(ImportCustomers(context, customers));

            //var saleReader = new StreamReader(@"E:\Кодене\CarDealer - Skeleton\CarDealer\Datasets\sales.xml");

            //var sales = saleReader.ReadToEnd();

            //Console.WriteLine(ImportSales(context, sales));

            Console.WriteLine(GetCarsWithTheirListOfParts(context));
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(List<ImportSupplier>), new XmlRootAttribute("Suppliers"));

            var importSuppliers = (List<ImportSupplier>)ser.Deserialize(new StringReader(inputXml));

            var suppliers = Mapper.Map<List<Supplier>>(importSuppliers);

            context.Suppliers.AddRange(suppliers);

            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(List<ImportPart>), new XmlRootAttribute("Parts"));

            var importParts = (List<ImportPart>)ser.Deserialize(new StringReader(inputXml));

            var parts = new List<Part>();

            foreach (var part in importParts)
            {
                if (context.Suppliers.Any(s => s.Id == part.SupplierId))
                {
                    var partz = Mapper.Map<Part>(part);

                    parts.Add(partz);
                }
            }

            context.Parts.AddRange(parts);

            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(List<ImportCar>), new XmlRootAttribute("Cars"));

            var importCars = (List<ImportCar>)ser.Deserialize(new StringReader(inputXml));

            var cars = Mapper.Map<List<Car>>(importCars);

            context.Cars.AddRange(cars);

            context.SaveChanges();

            foreach (var item in importCars)
            {
                var carId = context.Cars.First(c => c.Make == item.Make && c.Model == item.Model && c.TravelledDistance == item.TravelledDistance).Id;

                foreach (var partId in item.PartIds)
                {
                    if (context.Parts.Any(p => p.Id == partId.Id))
                    {
                        context.PartCars.Add(new PartCar
                        {
                            CarId = carId,
                            PartId = partId.Id

                        });
                    }
                }
            }

            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(List<ImportCustomer>), new XmlRootAttribute("Customers"));

            var importCustomers = (List<ImportCustomer>)ser.Deserialize(new StringReader(inputXml));

            var customers = Mapper.Map<List<Customer>>(importCustomers);

            context.Customers.AddRange(customers);

            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(List<ImportSale>), new XmlRootAttribute("Sales"));

            var importSales = (List<ImportSale>)ser.Deserialize(new StringReader(inputXml));

            var sales = new List<Sale>();

            foreach (var sale in importSales)
            {
                if (context.Cars.Any(c => c.Id == sale.CarId))
                {
                    var sal = Mapper.Map<Sale>(sale);

                    sales.Add(sal);
                }
            }

            context.Sales.AddRange(sales);

            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.TravelledDistance > 2000000)
                .Select(c => new CarWithDistance
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .ToList();

            var ser = new XmlSerializer(typeof(List<CarWithDistance>), new XmlRootAttribute("cars"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            var db = new StringBuilder();

            ser.Serialize(new StringWriter(db), cars, namespaces);

            return db.ToString().Trim();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var bmw = context.Cars
                .Where(c => c.Make == "BMW")
                .Select(c => new BWMs
                {
                    Id = c.Id,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .ToList();

            var ser = new XmlSerializer(typeof(List<BWMs>), new XmlRootAttribute("cars"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("", "")
            });

            var sb = new StringBuilder();

            ser.Serialize(new StringWriter(sb), bmw, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new LocalSupplier
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<LocalSupplier>), new XmlRootAttribute("suppliers"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            ser.Serialize(new StringWriter(sb), suppliers, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(c => new CarWithListOfParts
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance,
                    Parts = c.PartCars.Select(p => new PartFromList
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price
                    }).ToList()
                })
                .OrderByDescending(c => c.TravelledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<CarWithListOfParts>), new XmlRootAttribute("cars"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            ser.Serialize(new StringWriter(sb), cars, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new CustomerWithSales
                {
                    Name = c.Name,
                    Cars = c.Sales.Count,
                    SpentMoney = c.Sales.Sum(s => s.Car.Price)
                })
                .OrderByDescending(c => c.SpentMoney)
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<CustomerWithSales>), new XmlRootAttribute("cars"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            ser.Serialize(new StringWriter(sb), customers, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(s => new SaleWithDiscount
                {
                    Car = new CarForSale
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.Price,
                    PriceWithDiscount = Math.Round(s.Car.Price*(1-s.Discount/100),2)
                })
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<SaleWithDiscount>), new XmlRootAttribute("cars"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            ser.Serialize(new StringWriter(sb), sales, namespaces);

            return sb.ToString().Trim();
        }
    }
}