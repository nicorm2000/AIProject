﻿using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using VoronoiDiagram;

namespace Pathfinder.Voronoi
{
    public class Sector<TCoordinate, TCoordinateType>
        where TCoordinate : IEquatable<TCoordinate>, ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        private readonly List<Segment<TCoordinate, TCoordinateType>> segments = new();
        private readonly List<TCoordinate> intersections = new();
        private List<Node<TCoordinate>> nodesInsideSector = new();
        private List<TCoordinate> points;
        private static TCoordinate _wrongPoint;

        public Node<TCoordinateType> Mine { get; }

        public Sector(Node<TCoordinateType> mine)
        {
            _wrongPoint = new TCoordinate();
            _wrongPoint.SetCoordinate(-1, -1);
            this.Mine = mine;
        }

        #region SEGMENTS

        public void AddSegmentLimits(List<Limit<TCoordinate, TCoordinateType>> limits)
        {
            // Calculo los segmentos con los limites del mapa
            foreach (var limit in limits)
            {
                TCoordinate origin = new TCoordinate();
                origin.SetCoordinate(Mine.GetCoordinate()); // Obtengo la posicion de la mina
                TCoordinate final = limit.GetMapLimitPosition(origin); // Obtengo la posicion final del segmento
                segments.Add(new Segment<TCoordinate, TCoordinateType>(origin, final));
            }
        }

        public void AddSegment(TCoordinate origin, TCoordinate final)
        {
            segments.Add(new Segment<TCoordinate, TCoordinateType>(origin, final));
        }

        #endregion

        #region INTERSECTION

        public void SetIntersections()
        {
            intersections.Clear();

            // Calculo las intersecciones entre cada segmento (menos entre si mismo)
            for (int i = 0; i < segments.Count; i++)
            {
                for (int j = 0; j < segments.Count; j++)
                {
                    if (i == j) continue;

                    // Obtengo la interseccion
                    TCoordinate intersectionPoint = GetIntersection(segments[i], segments[j]);

                    if (intersectionPoint.Equals(_wrongPoint)) continue;

                    // Chequeo si esa interseccion ya existe
                    if (intersections.Contains(intersectionPoint)) continue;

                    // Calculo la distancia maxima entre la interseccion y el punto de oriden del segmento
                    float maxDistance = intersectionPoint.Distance(segments[i].Origin.GetCoordinate());

                    // Determino si la interseccion es valida
                    bool checkValidPoint = false;
                    for (int k = 0; k < segments.Count; k++)
                    {
                        // Recorro todos los segmentos de nuevo para verificar si hay otro
                        // segmento más cercano a la intersección que el segmento actual

                        // Esto es porque cada interseccion debe representar un punto donde los dos segmentos son los mas cercanos
                        // entre si, ya que cada punto en el plano pertenece a la region de voronoi de su segmento mas cercano
                        if (k == i || k == j) continue;
                        if (CheckIfHaveAnotherPositionCloser(intersectionPoint, segments[k].Final, maxDistance))
                        {
                            checkValidPoint = true; // Interseccion no valida
                            break;
                        }
                    }

                    if (!checkValidPoint)
                    {
                        intersections.Add(intersectionPoint);
                        segments[i].Intersections.Add(intersectionPoint);
                        segments[j].Intersections.Add(intersectionPoint);
                    }
                }
            }

            // Cada segmento debe tener exactamente dos intersecciones con otros segmentos, sino no es valido
            segments.RemoveAll((s) => s.Intersections.Count != 2);

            // Ordeno las intersecciones de acuerdo al angulo con respecto a un centro
            // determinado, sino los segmentos no se conectan bien y hay errores
            SortIntersections();

            // Creo un conjunto de puntos para definir los limites de los sectores
            SetPointsInSector();
        }

        
        // Calculo la interseccion entre 2 segmentos definidos por 4 puntos
        private TCoordinate GetIntersection(Segment<TCoordinate, TCoordinateType> seg1, Segment<TCoordinate, TCoordinateType> seg2)
        {
            
            
            /*
            Vector2 p1 = seg1.Mediatrix;
            Vector2 p2 = seg1.Mediatrix + seg1.Direction * MapGenerator.MapDimensions.magnitude; 

            Vector2 p3 = seg2.Mediatrix;
            Vector2 p4 = seg2.Mediatrix + seg2.Direction * MapGenerator.MapDimensions.magnitude;
             */
            
            TCoordinate intersection = new TCoordinate();
            intersection.Zero();

            TCoordinate p1 = seg1.Mediatrix;
            TCoordinate p2 = new TCoordinate();
            p2.SetCoordinate(seg1.Mediatrix.GetCoordinate());
            p2.Add(seg1.Direction.Multiply(MapGenerator<TCoordinate, TCoordinateType>.MapDimensions.GetMagnitude()));

            TCoordinate p3 = seg2.Mediatrix;
            TCoordinate p4 = new TCoordinate();
            p4.SetCoordinate(seg2.Mediatrix.GetCoordinate());
            p4.Add(seg2.Direction.Multiply(MapGenerator<TCoordinate,TCoordinateType>.MapDimensions.GetMagnitude()));

            // Chequeo si los dos segmentos son paralelos, si es asi no hay interseccion
            if (Approximately((p1.GetX() - p2.GetX()) * (p3.GetY() - p4.GetY()) -
                 (p1.GetY() - p2.GetY()) * (p3.GetX() - p4.GetX()), 0))
            {
                return _wrongPoint;
            }
            else
            {
                // Para calcular las coordenadas de la interseccion uso la formula de interseccion de dos lineas en el plano:
                /*
                   1.Ecuaciones de lineas en el plano:                             Ax + By = C1      y      Ax + By = C2
                   2. Calculo el determinante principal (D):                               D = A1 * B2 - A2 * B1
                   3. Calcula el determinante x (Dx):                                      Dx = C1 * B2 - C2 * B1
                   4. Calcula el determinante y (Dy):                                      Dy = A1 * C2 - A2 * C1
                   5. Calcula las coordenadas del punto de intersección (x, y):      x = Dx / D      y      y = Dy / D

                   A1 = p1.x - p2.x
                   B1 = p1.y - p2.y
                   A2 = p3.x - p4.x
                   B2 = p3.y - p4.y
                   C1 = p1.x * p2.y - p1.y * p2.x
                   C2 = p3.x * p4.y - p3.y * p4.x
                */
                intersection.SetX(
                    ((p1.GetX() * p2.GetY() - p1.GetY() * p2.GetX()) * (p3.GetX() - p4.GetX()) -
                     (p1.GetX() - p2.GetX()) * (p3.GetX() * p4.GetY() - p3.GetY() * p4.GetX())) /
                    ((p1.GetX() - p2.GetX()) * (p3.GetY() - p4.GetY()) -
                     (p1.GetY() - p2.GetY()) * (p3.GetX() - p4.GetX())));
                intersection.SetY(
                    ((p1.GetX() * p2.GetY() - p1.GetY() * p2.GetX()) * (p3.GetY() - p4.GetY()) -
                     (p1.GetY() - p2.GetY()) * (p3.GetX() * p4.GetY() - p3.GetY() * p4.GetX())) /
                    ((p1.GetX() - p2.GetX()) * (p3.GetY() - p4.GetY()) -
                     (p1.GetY() - p2.GetY()) * (p3.GetX() - p4.GetX())));
                return intersection;
            }
        }

        private bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
        
        private bool CheckIfHaveAnotherPositionCloser(TCoordinate intersectionPoint, TCoordinate pointEnd,
            float maxDistance)
        {
            return (intersectionPoint.Distance(pointEnd.GetCoordinate()) < maxDistance);
        }

        private void SortIntersections()
        {
            List<IntersectionPoint<TCoordinate>> intersectionPoints = 
                intersections.Select(coord => new IntersectionPoint<TCoordinate>(coord)).ToList();

            // Calculo los valores maximos y minimos de X e Y de las intersecciones para determinar el punto central (centroide)
            float minX = intersectionPoints[0].Position.GetX();
            float maxX = intersectionPoints[0].Position.GetX();
            float minY = intersectionPoints[0].Position.GetY();
            float maxY = intersectionPoints[0].Position.GetY();

            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersectionPoints[i].Position.GetX() < minX) minX = intersectionPoints[i].Position.GetX();
                if (intersectionPoints[i].Position.GetX() > maxX) maxX = intersectionPoints[i].Position.GetX();
                if (intersectionPoints[i].Position.GetY() < minY) minY = intersectionPoints[i].Position.GetY();
                if (intersectionPoints[i].Position.GetY() > maxY) maxY = intersectionPoints[i].Position.GetY();
            }

            TCoordinate center = new TCoordinate();
            center.SetCoordinate(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);

            // Calculo el angulo de cada interseccion con respecto al punto central:
            // calculo el angulo en radianes entre el punto de interseccion y un punto central con el eje horizontal
            foreach (var coord in intersectionPoints)
            {
                TCoordinate pos = coord.Position;
                coord.Angle = (float)Math.Acos((pos.GetX() - center.GetX()) /
                                           Math.Sqrt(Math.Pow(pos.GetX() - center.GetX(), 2f) +
                                                     Math.Pow(pos.GetY() - center.GetY(), 2f)));

                // Si la coordenada Y de la interseccion es mayor que la coordenada Y del centro, ajusto el angulo para
                // garantizar que este en el rango correct (0 a 2pi radianes)
                if (pos.GetY() > center.GetY())
                    coord.Angle = (float)(Math.PI + Math.PI - coord.Angle);
            }

            // Ordeno las interseccion en funcion de sus angulos (ascendente, en sentido anti-horario)
            intersectionPoints = intersectionPoints.OrderBy(p => p.Angle).ToList();
            intersections.Clear();

            foreach (var coord in intersectionPoints)
            {
                intersections.Add(coord.Position);
            }
        }

        private void SetPointsInSector()
        {
            points = new List<TCoordinate>();
            foreach (var coord in intersections)
            {
                // Asigno cada interseccion como un punto
                points.Add(coord);
            }

            // Se crea un punto adicional que es igual al primer punto, para completar el limite del ultimo sector
            points.Add(points[0]);
        }

        #endregion

        public bool CheckPointInSector(TCoordinate position) // Calculo si "position" esta dentro de un sector del diagrama
        {
            if (points == null) return false;

            bool inside = false;

            // Inicializo "point" con el ultimo punto (^1) de la matriz "points"
            TCoordinate point = new TCoordinate();
            point.SetCoordinate(points[^1].GetCoordinate());

            foreach (var coord in points)
            {
                // Guardo el valor X e Y del punto anterior y el punto actual
                float previousX = point.GetX();
                float previousY = point.GetY();
                point.SetCoordinate(coord.GetCoordinate());

                // (El operador ^ alterna el valor del bool)
                // Calculo si "position" cruza o no una línea formada por dos puntos consecutivos en el polígono:
                // 1. Verifico si "position" esta por debajo de los puntos actual y anterior en el eje vertical (1 sola comparacion es V = V)
                // 2. Verifico si "position" esta a la izquierda de la linea que conecta los puntos actual y anterior
                bool condition1 = point.GetY() > position.GetY() ^ previousY > position.GetY();
                bool condition2 = (position.GetX() - point.GetX()) <
                                  (position.GetY() - point.GetY()) * (previousX - point.GetX()) /
                                  (previousY - point.GetY());

                // Si ambas condiciones son verdaderas, el punto está fuera del polígono
                inside ^= condition1 && condition2;
            }

            return inside;
        }

        public List<Node<TCoordinate>> GetNodesInSector(List<Node<TCoordinate>> allNodes)
        {
            List<Node<TCoordinate>> nodesInSector = new List<Node<TCoordinate>>();

            foreach (Node<TCoordinate> node in allNodes)
            {
                if (CheckPointInSector(node.GetCoordinate()))
                {
                    nodesInSector.Add(node);
                }
            }

            return nodesInSector;
        }

        public int CalculateTotalWeight(List<Node<TCoordinate>> nodesInSector)
        {
            int totalWeight = 0;

            foreach (Node<TCoordinate> node in nodesInSector)
            {
                // TODO totalWeight += node.GetPathNodeCost();
                totalWeight += 1;
            }

            return totalWeight;
        }

        public TCoordinate[] PointsToDraw()
        {
            return points.ToArray();
        }
    }
}