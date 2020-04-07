using System;
using System.Collections;
using NetTopologySuite.Geometries;
using OSMLSGlobalLibrary;
using OSMLSGlobalLibrary.Map;
using OSMLSGlobalLibrary.Modules;
using System.Collections.Generic;
using System.Linq;

namespace TestModule
{

    static class Rand // Рандом
    {
        private static Random rand = new Random();

        public static int GenerateInRange(int min, int max) => (int)Math.Round(min - 0.5 + rand.NextDouble() * (max - min + 1));
        public static Coordinate GenerateNext((int leftX, int rightX, int downY, int upY) map) => new Coordinate(GenerateInRange(map.leftX, map.rightX), GenerateInRange(map.downY, map.upY));
    }

    public class TestModule_2 : OSMLSModule
    {
        public (int leftX, int rightX, int downY, int upY) map;

     

 
        Place place; // Дикая местность


        protected override void Initialize()
        {
            //Координаты границ дикой местности
            var leftX = 5040901;
            var rightX = 5110937;
            var downY = 6234004;
            var upY = 6288083;
            map = (leftX, rightX, downY, upY);

       

            var polygonCoordinates = new Coordinate[] {
                    new Coordinate(leftX, downY),
                    new Coordinate(leftX, upY),
                    new Coordinate(rightX, upY),
                    new Coordinate(rightX, downY),
                    new Coordinate(leftX, downY),
            };

            place = new Place(new LinearRing(polygonCoordinates));

            MapObjects.Add(place);



            var countWolf = 10;
            for (var i = 0; i < countWolf; i++)
            {
                MapObjects.Add(new Wolf(Rand.GenerateNext(map), 40, "male"));
                MapObjects.Add(new Wolf(Rand.GenerateNext(map), 30, "female"));
                

            }
            var countDeer = 15;
            for (var i = 0; i < countDeer; i++)
            {
                MapObjects.Add(new Deer(Rand.GenerateNext(map), 29, "male"));
                MapObjects.Add(new Deer(Rand.GenerateNext(map), 9, "female"));
                MapObjects.Add(new Deer(Rand.GenerateNext(map), 9, "female"));

            }

        }

       

        public override void Update(long elapsedMilliseconds)
        {


            var deers = MapObjects.GetAll<Deer>();
            Deer[] deers1 = deers.ToArray<Deer>();


            var wolfs = MapObjects.GetAll<Wolf>();
            Wolf[] wolfs1 = wolfs.ToArray<Wolf>();






            //Создаем две группы самок и самцов


            List<Wolf> w_m = new List<Wolf> { };
            List<Wolf> w_f = new List<Wolf> { };

            List<Deer> d_m = new List<Deer> { };
            List<Deer> d_f = new List<Deer> { };

            List<Deer> d_f_end = new List<Deer> { };
            List<Wolf> w_f_end = new List<Wolf> { };

            foreach (var wolf in wolfs1)
            {
                if (wolf.Sex == "male" && wolf.NonBreedingPeriod == true)
                {
                    w_m.Add(wolf);
                } else if (wolf.Sex == "female" && wolf.NonBreedingPeriod == true)
                {
                    w_f.Add(wolf);
                }
                else if (wolf.Sex == "female" && wolf.NonBreedingPeriod == false)
                {
                    w_f_end.Add(wolf);
                }

            }

            foreach (var deer in deers1)
            {
                if (deer.Sex == "male" && deer.NonBreedingPeriod == true)
                {
                    d_m.Add(deer);
                }
                else if (deer.Sex == "female" && deer.NonBreedingPeriod == true)
                {
                    d_f.Add(deer);
                }
                else if (deer.Sex == "female" && deer.NonBreedingPeriod == false)
                {
                    d_f_end.Add(deer);
                }
            }

           





            if (w_f.Count != 0) { 
            //Размножаем волков
            foreach (var wolf in w_m)
            {

                var nearestWolfForWolf = w_f.Aggregate((wolfFem1, wolfFem2) => wolf.distance(wolfFem1) < wolf.distance(wolfFem2) ? wolfFem1 : wolfFem2);
                wolf.Move(new Coordinate(nearestWolfForWolf.X, nearestWolfForWolf.Y));

                if (wolf.CreateNewWolf(nearestWolfForWolf) && wolf.NonBreedingPeriod == true)
                {
                    MapObjects.Add(new Wolf(new Coordinate(nearestWolfForWolf.X + 1000, nearestWolfForWolf.Y + 1000), 10, "male"));
                    wolf.X = nearestWolfForWolf.X - 1000;
                    wolf.Y = nearestWolfForWolf.Y - 1000;
                    w_f.Remove(nearestWolfForWolf);

                    nearestWolfForWolf.NonBreedingPeriod = false;

                }
            }
        }


                //Кушаем оленей
                foreach (var wolf in wolfs)
               {
                 var nearestDeer = deers.Aggregate((deer1, deer2) => wolf.distance(deer1) < wolf.distance(deer2) ? deer1 : deer2);          
                 wolf.Move(new Coordinate(nearestDeer.X, nearestDeer.Y));
                    if (wolf.CanEat(nearestDeer))
                    {
                    MapObjects.Remove(nearestDeer);
                    deers.Remove(nearestDeer);                     
                    }           
                }



            foreach (var deer in d_f)
             deer.MoveByMap(map);

            foreach (var deer in d_f_end)
                deer.MoveByMap(map);



            //Размножаем оленей
            foreach (var deer in d_m)
            {
                

                var nearestDeerForDeer = d_f.Aggregate((deerFem1, deerFem2) => deer.distance(deerFem1) < deer.distance(deerFem2) ? deerFem1 : deerFem2);
                deer.Move(new Coordinate(nearestDeerForDeer.X, nearestDeerForDeer.Y));

                if (deer.CreateNewDeer(nearestDeerForDeer) && deer.NonBreedingPeriod == true)
                {
                    MapObjects.Add(new Deer(new Coordinate(nearestDeerForDeer.X + 1000, nearestDeerForDeer.Y + 1000), 10, "male"));
                    deer.X = nearestDeerForDeer.X - 1000;
                    deer.Y = nearestDeerForDeer.Y - 1000;
                    d_f.Remove(nearestDeerForDeer);
                    
                    nearestDeerForDeer.NonBreedingPeriod = false;
                   

                }
            }






            w_f.Clear();
            w_m.Clear();

            d_f.Clear();
            d_m.Clear();

            d_f_end.Clear();
            w_f_end.Clear();
        }
    }
    public static class PointExtension
    {
        public static double distance(this Point p1, Point p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        public static double distance(this Point p1, Coordinate p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

        public static void Move(this Point p, Coordinate direction, double speed)
        {
            double MinimumDirection(double s, double d) =>
                Math.Min(speed, Math.Abs(s - d)) * Math.Sign(d - s);

            p.X += MinimumDirection(p.X, direction.X);
            p.Y += MinimumDirection(p.Y, direction.Y);
        }
    }

    #region объявления класса, унаследованного от точки, объекты которого будут иметь уникальный стиль отображения на карте

    /// <summary>
    /// Волк, умеющий передвигаться вверх-вправо с заданной скоростью.
    /// </summary>
    [CustomStyle(
        @"new ol.style.Style({
            image: new ol.style.Circle({
                opacity: 1.0,
                scale: 1.0,
                radius: 5,
                fill: new ol.style.Fill({
                    color: 'rgba(255, 0, 0, 0.9)'
                }),
                stroke: new ol.style.Stroke({
                    color: 'rgba(0, 0, 0, 0.4)',
                    width: 1
                }),
            })
        });
        ")] 
    class Wolf : Point // Унаследуем данный данный класс от стандартной точки.
    {
        
        public double Speed { get; }
        public string Sex { get; set; }

        public  bool NonBreedingPeriod { get; set; }

        private Coordinate destinationPoint = null;

  

        public Wolf(Coordinate coordinate, double speed, string sex) : base(coordinate)
        {
            Speed = speed;
            Sex = sex;
            NonBreedingPeriod = true;
          
        }
        public void Move(Coordinate direction)
        {
            this.Move(direction,Speed);
        }
        public void MoveByMap((int leftX, int rightX, int downY, int upY) map)
        {
            if (destinationPoint == null || this.distance(destinationPoint) < Speed)
            {
                destinationPoint = Rand.GenerateNext(map);
            }
            Move(destinationPoint);
        }
        public bool CanEat(Deer deer)
        {
            
            return this.distance(deer) < Speed;
            
        }

        public bool CreateNewWolf(Wolf wolf)
        {
                   
                return this.distance(wolf) < Speed;
               

        }

        


    }
    [CustomStyle(
       @"new ol.style.Style({
            image: new ol.style.Circle({
                opacity: 1.0,
                scale: 1.0,
                radius: 4,
                fill: new ol.style.Fill({
                    color: 'rgba(0, 255, 0, 0.9)'
                }),
                stroke: new ol.style.Stroke({
                    color: 'rgba(0, 0, 0, 0.4)',
                    width: 1
                }),
            })
        });
        ")] // Переопределим стиль всех объектов данного класса, сделав самолет фиолетовым, используя атрибут CustomStyle.
    class Deer : Point // Унаследуем данный данный класс от стандартной точки.
    {
        public double Speed { get; } // Скорость передвижения
        public string Sex { get; } // Пол
        private Coordinate destinationPoint = null;

        public bool NonBreedingPeriod = true; //Способность размножаться
        public Deer(Coordinate coordinate, double speed, string sex) : base(coordinate)
        {
            Speed = speed;
            Sex = sex;
            NonBreedingPeriod = true;
        }

        public void MoveByMap((int leftX, int rightX, int downY, int upY) map)
        {
            if (destinationPoint == null || this.distance(destinationPoint) < Speed)
            {
                destinationPoint = Rand.GenerateNext(map);
            }
            Move(destinationPoint);
        }
        public void Move(Coordinate direction)
        {
            this.Move(direction, Speed);
                     
        }
        public bool CreateNewDeer(Deer deer)
        {
          
                return this.distance(deer) < Speed;
          
                    
        }
    }



        //Класс - дикой местности
        class Place : Polygon//// Унаследуем данный данный класс от стандартного полигона.
       {

        public Place(LinearRing shell) : base(shell) { 
        
        }

        

        
       
    }
   

    #endregion
}
