// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {

        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Where - Task 1")]
        [Description("1.Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X. Продемонстрируйте выполнение запроса с различными X (подумайте, можно ли обойтись без копирования запроса несколько раз)")]
        public void LinqTask1()
        {
            decimal total = 80000;

            // linq
            var clients = dataSource.Customers.Where(o => o.Orders.Sum(s => s.Total) > total);

            // linq to SQL
            var clientsSQL = from customer in dataSource.Customers
                             where customer.Orders.Sum(i => i.Total) > total
                             select customer;

            foreach (var c in clients)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 2")]
        [Description("2.Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. Сделайте задания с использованием группировки и без.")]

        public void LinqTask2()
        {
            // linqSQL
            var resultlinqSQL = from customer in dataSource.Customers
                                from suppliers in dataSource.Suppliers
                                where customer.Country == suppliers.Country && customer.City == suppliers.City
                                select new
                                {
                                    customer.CompanyName,
                                    suppliers.SupplierName,
                                    customer.Country,
                                    customer.City

                                };

            // linqSQL
            var result = dataSource.Customers.SelectMany(
                suppliers => dataSource.Suppliers.Where(customer =>
                    customer.Country == suppliers.Country && customer.City == suppliers.City),
                (customer, supplier) =>
                    new { customer.CompanyName, supplier.SupplierName, supplier.Country, supplier.City });

            var resultlinqSQL2 = from customer in dataSource.Customers
                select new 
                {
                    Customer =  customer.CompanyName,
                    Supplier = dataSource.Suppliers.Where(s=>s.Country == customer.Country && s.City == customer.City)
                };

            // linqSQL
            var result2 = dataSource.Customers.Select(c => new
            {
                Customer = c.CompanyName,
                Supplier = dataSource.Suppliers.Where(s => s.Country == c.Country && s.City == c.City)
            });

            var resultGroupBy = dataSource.Customers.Select(c => new
            {
                Customer = c.CompanyName,
                Supplier = dataSource.Suppliers.GroupBy(g => new {g.Country, g.City})
                    .FirstOrDefault(t => t.Key.City == c.City && t.Key.Country == c.Country)
            });

            foreach (var c in resultlinqSQL2)
            {
                ObjectDumper.Write(c);

                foreach (var product in c.Supplier)
                {
                    ObjectDumper.Write($"{product.SupplierName}, {product.Country}, {product.City}");
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 3")]
        [Description("3.Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]

        public void LinqTask3()
        {
            var max = 10000;

            // Linq
            var result = dataSource.Customers.Where(c => c.Orders.Any(t => t.Total > max)).Select(c => c);

            var resultSQL = from client in dataSource.Customers
                            where client.Orders.Any(t => t.Total > max)
                            select client;

            var result3 = dataSource.Customers.Where(c => c.Orders.Any(t => t.Total > max)).Select(c => c);

            foreach (var c in result)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 4")]
        [Description("4.	Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами (принять за таковые месяц и год самого первого заказа)")]

        public void LinqTask4()
        {
            var max = 10000;

            // LinqSQL
            var resultSQL = from client in dataSource.Customers
                            where client.Orders.Length >=1
                            select new
                            {
                                CompanyName = client.CompanyName,
                                firstDate = client.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate
                            };

            // Linq
            var result = dataSource.Customers.Where(c => c.Orders.Length >= 1).Select(cust => new 
            {
                CompanyName = cust.CompanyName,
                firstDate = cust.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate
            });
  
            foreach (var c in result)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 5")]
        [Description("5.	Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу, оборотам клиента (от максимального к минимальному) и имени клиента")]

        public void LinqTask5()
        {
            var max = 10000;

            // Linq
            var result = dataSource.Customers.Where(c => c.Orders.Length >= 1).Select(cust => new
            {
                CompanyName = cust.CompanyName,
                FirstDate = cust.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate,
                TurnOver = cust.Orders.Sum(i => i.Total)
            }).OrderBy(d => d.FirstDate).ThenByDescending(c => c.TurnOver).ThenBy(n => n.CompanyName);

            // LinqSQL
            var resultSQL = from client in dataSource.Customers
                where client.Orders.Length >= 1
                select new
                {
                    CompanyName = client.CompanyName,
                    firstDate = client.Orders.OrderBy(o => o.OrderDate).FirstOrDefault()?.OrderDate,
                    TurnOver = client.Orders.Sum(i => i.Total)
                }
                into client
                orderby client.firstDate, client.TurnOver descending, client.CompanyName
                select client;

            foreach (var c in result)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 6")]
        [Description("6. Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]

        public void LinqTask6()
        {
            var post = "[0-9]";
            var phoneReg = @"^\(\d+\)";


            var result = from customer in dataSource.Customers
                where customer.Region == null || customer.PostalCode == null || Regex.IsMatch(post, customer.PostalCode) != true || Regex.IsMatch(phoneReg, customer.Phone) != true
                select new {customer.CompanyName, customer.Region, customer.Phone, customer.PostalCode};


            foreach (var c in result)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 7")]
        [Description("7.	Сгруппируйте все продукты по категориям, внутри – по наличию на складе, внутри последней группы отсортируйте по стоимости")]

        public void LinqTask7SQL()
        {
            var resultSQL = from product in dataSource.Products
                group product by product.Category
                into prodGroup
                select new
                {
                    Category = prodGroup.Key,
                    UnitsInStock = from pr in prodGroup
                        group pr by pr.UnitsInStock into uGroup
                        select new
                        {
                            Key = uGroup.Key,
                            Product = uGroup.OrderBy(p => p.UnitPrice)
                        }
                };

            var result = dataSource.Products.GroupBy(x => x.Category).Select(o => new
            {
                Category = o.Key,
                UnitsInStock = o.GroupBy(u => u.UnitsInStock).Select(p => new
                {
                    Key = p.Key,
                    Product = p.OrderBy(pr => pr.UnitPrice)
                })
            });

            foreach (var c in result)
            {
                ObjectDumper.Write(c);
                foreach (var product in c.UnitsInStock)
                {
                    ObjectDumper.Write(product.Key);
                    foreach (var p in product.Product)
                    {
                        ObjectDumper.Write($"{p.ProductName}, {p.UnitPrice}");
                    }
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 8")]
        [Description("8. Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». Границы каждой группы задайте сами")]

        public void LinqTask8()
        {
            var lowPrice = 10.0000M;
            var middlePrice = 15.0000M;

            var resultSQL = from product in dataSource.Products
                group product by new
                {
                    LowPrise = product.UnitPrice <= lowPrice,
                    MiddlePrise = product.UnitPrice > lowPrice && product.UnitPrice <= middlePrice,
                    ExpensivePrice = product.UnitPrice > middlePrice
                };

            var result = dataSource.Products.GroupBy(product => new
            {
                LowPrise = product.UnitPrice <= lowPrice,
                MiddlePrise = product.UnitPrice > lowPrice && product.UnitPrice <= middlePrice,
                ExpensivePrice = product.UnitPrice > middlePrice
            });

            foreach (var c in resultSQL)
            {
                ObjectDumper.Write(c.Key);
                foreach (var u in c)
                {
                    ObjectDumper.Write($"{u.ProductName}, {u.UnitPrice}");
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 9")]
        [Description("9.Рассчитайте среднюю прибыльность каждого города (среднюю сумму заказа по всем клиентам из данного города) и среднюю интенсивность (среднее количество заказов, приходящееся на клиента из каждого города)")]

        public void LinqTask9()
        {
            var result = from c in dataSource.Customers
                group c by c.City into groupSity
                select new
                {
                    groupSity = groupSity.Key,
                    AverProfit = groupSity.Select(p => p.Orders.Sum(i => i.Total)).Sum(i => i) /
                                 groupSity.Select(i => i).Count(),
                    AverCountOfOrder = groupSity.Select(p => p.Orders.Select(o => o.OrderID).Count()).Sum(o => o)
                };

            foreach (var city in result)
            {
                ObjectDumper.Write($"{city.groupSity}, {city.AverCountOfOrder}, {city.AverProfit}");
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 10")]
        [Description("10.	Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), статистику по годам, по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]

        public void LinqTask10()
        {
            var resultByMonth = from customer in dataSource.Customers
                from order in customer.Orders
                group order by order.OrderDate.Month into groupByMonth
                select new
                {
                    groupMonth = groupByMonth.Key,
                    Orders = groupByMonth.Count()
                };

            var resultByYear = from customer in dataSource.Customers
                from order in customer.Orders
                group order by order.OrderDate.Year into groupByYear
                select new
                {
                    groupMonth = groupByYear.Key,
                    Orders = groupByYear.Count()
                };

            var resultByYearByMonth = from customer in dataSource.Customers
                from order in customer.Orders
                group order by order.OrderDate.Year into groupByYear
                select new
                {
                    groupYear = groupByYear.Key,
                    Month = from o in groupByYear
                        group o by o.OrderDate.Month into monthGroup
                            select  new
                            {
                                groupMonth = monthGroup.Key,
                                Orders = monthGroup.Count()
                            }
                };

            foreach (var c in resultByYearByMonth)
            {
                ObjectDumper.Write(c.groupYear);
                foreach (var m in c.Month)
                {
                    ObjectDumper.Write($"{m.groupMonth}, {m.Orders}");
                }
            }
        }
    }
}
