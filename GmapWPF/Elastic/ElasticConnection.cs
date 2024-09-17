using Elastic.Clients.Elasticsearch;

using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Elastic.Transport.Extensions;
using GMap.NET;
using GmapWPF.Models;
using System;
using System.Diagnostics;

namespace MusteriTakipWithElasticSearch.Elastic
{

    public class ElasticConnection
    {
        private const string IndexName = "location";
        public ElasticConnection() { }
        public ElasticsearchClient CreateConnection()
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
           .Authentication(new BasicAuthentication("elastic", "changeme"));

            var client = new ElasticsearchClient(settings);

            if (client == null)
            {
                throw new Exception("Connection Başarısız Oldu");
            }

            else
            {
                return client;
            }
        }
        public async Task<List<Location>> ElasticSearchQuery(ElasticsearchClient _client, List<PointLatLng> points)
        {
            //GeoShapeFieldQuery geoShapeFieldQuery = new GeoShapeFieldQuery();
            //geoShapeFieldQuery.Shape = "polygon";
            //geoShapeFieldQuery.Relation = GeoShapeRelation.Within; // alanında içindekileri alır.

            List<GeoPoint> locations = new List<GeoPoint>();
            foreach (var point in points)
            {
                locations.Add(new GeoPoint()
                {
                    Lat = point.Lat,
                    Lon = point.Lng
                });
            }
            var coordinates = new List<List<double>>
{
    new List<double> { 30.52, 39.76 }, // lon, lat
    new List<double> { 32.858262062072754, 39.92214537664712 },
    new List<double> { 32.85594463348389, 39.91671459644197 },
    new List<double> { 32.84886360168457, 39.92286944813247 },
    new List<double> { 30.52, 39.76 } // Poligonun kapatılması için tekrar ilk nokta
};




            Debug.WriteLine(locations);

            var geoShapeFieldQuery = new GeoShapeFieldQuery
            {
                Shape = new
                {
                    Type = "polygon",
                    Coordinates = new List<List<List<double>>>
                  {
                coordinates
                   },
                },

                Relation = GeoShapeRelation.Within
            };

            var existIndex = _client.Indices.ExistsAsync(IndexName);

            if (existIndex == null)
            {
                return new List<Location>();
            }

            var result = new SearchRequestDescriptor<Location>(IndexName);

            //var last =  result.Index(IndexName).Query(q => q
            //.Bool(b => b
            //.Must(m => m.
            //Match(ma => ma.
            //Field(f => f.text).Query("Geopoint")))));

            var last = result.Index(IndexName).Query(q => q
              .GeoShape(g => g
                  .Field(f => f.geoPoint)
                  .Shape(geoShapeFieldQuery)
                  .IgnoreUnmapped()));


            var finalResult = await _client.SearchAsync(last);

            var jsonQuery = _client.RequestResponseSerializer.SerializeToString(last, SerializationFormatting.Indented);

            Console.WriteLine(jsonQuery);
            //var searchResult = await _client.SearchAsync<Location>(s => s
            //.Index(IndexName)
            //  .Query(q => q
            //  .GeoShape(g => g
            //      .Field(f => f.geoPoint)
            //      .Shape(geoShapeFieldQuery)
            //      .IgnoreUnmapped())
            //));


            return finalResult.Documents.ToList();
        }
    }
}
