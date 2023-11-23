using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs;
using CarDealer.Models;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();

            //9. Import Suppliers
            //string suppliersJson = File.ReadAllText("../../../Datasets/suppliers.json");
            //Console.WriteLine(ImportSuppliers(context, suppliersJson));

            //10. Import Parts
            //string partsJson = File.ReadAllText("../../../Datasets/parts.json");
            //Console.WriteLine(ImportParts(context, partsJson));

            //11. Import Cars
            //string carsJson = File.ReadAllText("../../../Datasets/cars.json");
            //Console.WriteLine(ImportCars(context, carsJson));

            //12. Import Customers
            //string customersJson = File.ReadAllText("../../../Datasets/customers.json");
            //Console.WriteLine(ImportCustomers(context, customersJson));

            //13. Import Sales
            //string salesJson = File.ReadAllText("../../../Datasets/sales.json");
            //Console.WriteLine(ImportSales(context, salesJson));

            //14. Export Ordered Customers
            //Console.WriteLine(GetOrderedCustomers(context));

            //15. Export Cars from Make Toyota
            //Console.WriteLine(GetCarsFromMakeToyota(context));

            //16. Export Local Suppliers
            //Console.WriteLine(GetLocalSuppliers(context));

            //17. Export Cars with Their List of Parts
            //Console.WriteLine(GetCarsWithTheirListOfParts(context));

            //18. Export Total Sales by Customer
            Console.WriteLine(GetTotalSalesByCustomer(context));
        }

        // 9. Import suppliers

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CarDealerProfile>());
            IMapper mapper = new Mapper(config);

            SupplierDTO[] supplierDTOs = JsonConvert.DeserializeObject<SupplierDTO[]>(inputJson);

            Supplier[] suppliers = mapper.Map<Supplier[]>(supplierDTOs);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}.";
        }

        //10. Import Parts
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CarDealerProfile>());
            IMapper mapper = new Mapper(config);

            PartsDTO[] partDTOs = JsonConvert.DeserializeObject<PartsDTO[]>(inputJson);
            List<Part> parts = new List<Part>();
            foreach (var partDTO in partDTOs)
            {
                if (context.Suppliers.Any(s => s.Id == partDTO.SupplierId))
                {
                    parts.Add(
                        mapper.Map<Part>(partDTO));
                }
            }

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count()}.";
        }

        //11. Import Cars

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CarDealerProfile>());
            IMapper mapper = new Mapper(config);

            CarsDTO[] carDtos = JsonConvert.DeserializeObject<CarsDTO[]>(inputJson)!;

            List<Car> cars = new();

            foreach (var carDto in carDtos)
            {
                Car car = mapper.Map<Car>(carDto);

                foreach (int partId in carDto.partsId.Distinct())
                {
                    car.PartsCars.Add(new PartCar()
                    {
                        PartId = partId
                    });
                }

                cars.Add(car);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}.";
        }

        //11. Import Customers
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CarDealerProfile>());
            IMapper mapper = new Mapper(config);

            CustomersDTO[] customersDTOs = JsonConvert.DeserializeObject<CustomersDTO[]>(inputJson);

            Customer[] customers = mapper.Map<Customer[]>(customersDTOs);

            context.Customers.AddRange(customers);
            context.SaveChanges();
            return $"Successfully imported {customers.Length}.";
        }

        //13. Import Sales
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CarDealerProfile>());
            IMapper mapper = new Mapper(config);

            SalesDTO[] salesDTOs = JsonConvert.DeserializeObject<SalesDTO[]>(inputJson);

            Sale[] sales = mapper.Map<Sale[]>(salesDTOs);

            context.Sales.AddRange(sales);
            context.SaveChanges();
            return $"Successfully imported {sales.Length}.";
        }

        //14. Export Ordered Customers
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver != false);

            string output = JsonConvert.SerializeObject(
                customers.Select(c => new
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver,
                }),
                Formatting.Indented);
            return output;
        }
        //15. Export Cars from Make Toyota
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance);

            string output = JsonConvert.SerializeObject(cars.Select(c => new
            {
                c.Id,
                c.Make,
                c.Model,
                c.TraveledDistance
            }),
            formatting: Formatting.Indented);

            return output;
        }
        //16. Export Local Suppliers
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    PartsCount = c.Parts.Count
                });

            string output = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return output;
        }
        //17. Export Cars with Their List of Parts
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var Cars = context.Cars
                 .Select(c => new
                 {
                     car = new
                     {
                         c.Make,
                         c.Model,
                         c.TraveledDistance
                     },
                     parts = c.PartsCars.Select(cp => new
                     {
                         Name = cp.Part.Name,
                         Price = $"{cp.Part.Price:f2}"
                     })
                 });
            string output = JsonConvert.SerializeObject (Cars, Formatting.Indented);
            return output;
        }
        //18. Export Total Sales by Customer
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var FinalCustomers = new List<object>();
            var customers = context.Customers.ToList();
            string output = "";

            foreach (var customer1 in customers)
            {
                var spentMoney1 = 0m;
                var sales = context.Sales
                    .Where(s => s.Customer.Id == customer1.Id && s.Customer != null).ToList();

                foreach (var sale in sales)
                {
                    var car =  context.Cars
                        .Where(c => c.Id == sale.CarId).FirstOrDefault();

                    var partsCars = context.PartsCars
                        .Where(pc => pc.CarId == sale.CarId).ToList();

                    int[] partIds = partsCars.Select(x => x.PartId).ToArray();

                    var parts = context.Parts.Where(p => partIds.Contains(p.Id));
                        
                        
                        
                    if (customer1.IsYoungDriver)
                    {
                        spentMoney1 += parts.Sum(p => p.Price * p.Quantity) - sale.Discount - 5;
                    }
                    else
                    {
                        spentMoney1 += car.PartsCars.Sum(p => p.Part.Price * p.Part.Quantity) - sale.Discount;
                    }                   
                }
                var customer = context.Customers
                        .Where(c => c.Id == customer1.Id).Select(c => new
                        {
                            fullName = customer1.Name,
                            boughtCars = customer1.Sales.Count,
                            spentMoney = spentMoney1.ToString("f2")
                        }).ToList()
                        .OrderByDescending(c => c.spentMoney)
                        .ThenByDescending(c => c.boughtCars);
               
                FinalCustomers.Add(customer);
            }
            output += JsonConvert.SerializeObject(FinalCustomers, Formatting.Indented);

            return output;
        }
    }
}