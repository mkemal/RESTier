﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.Restier.WebApi.Test.Services.Trippin.Domain;
using Microsoft.Restier.WebApi.Test.Services.Trippin.Models;

namespace Microsoft.Restier.WebApi.Test.Services.Trippin.Controllers
{
    public class TrippinController : ODataDomainController<TrippinDomain>
    {
        private TrippinModel DbContext
        {
            get
            {
                return Domain.Context;
            }
        }

        /// <summary>
        /// TODO: This method is for actual executing.
        /// </summary>
        [ODataRoute("ResetDataSource")]
        public void ResetDataSource()
        {
            TrippinModel.ResetDataSource();
        }

        [ODataRoute("CleanUpExpiredTrips")]
        public void CleanUpExpiredTrips()
        {
            Domain.CleanUpExpiredTrips();
        }

        [ODataRoute("Trips({key})/Microsoft.Restier.WebApi.Test.Services.Trippin.Models.EndTrip")]
        public IHttpActionResult EndTrip(int key)
        {
            return Ok(Domain.EndTrip(key));
        }

        private bool PeopleExists(int key)
        {
            return DbContext.People.Any(p => p.PersonId == key);
        }

        private string GetServiceRootUri()
        {
            var routeName = Request.ODataProperties().RouteName;
            ODataRoute odataRoute = Configuration.Routes[routeName] as ODataRoute;
            var prefixName = odataRoute.RoutePrefix;
            var requestUri = Request.RequestUri.ToString();
            var serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName, StringComparison.InvariantCultureIgnoreCase) + prefixName.Length);
            return serviceRootUri;
        }

        [ODataRoute("People/$count")]
        public IHttpActionResult GetPeopleCount()
        {
            return Ok(DbContext.People.Count());
        }

        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/LastName/$value")]
        public string GetPersonLastName([FromODataUri]int key)
        {
            return DbContext.People.Where(c => c.PersonId == key).Select(c => c.LastName).FirstOrDefault();
        }

        [ODataRoute("People({key})/Microsoft.Restier.WebApi.Test.Services.Trippin.Models.GetNumberOfFriends")]
        public IHttpActionResult GetNumberOfFriends([FromODataUri]int key)
        {
            return Ok(Domain.GetNumberOfFriends(key));
        }

        [ODataRoute("GetPersonWithMostFriends")]
        public IHttpActionResult GetPersonWithMostFriends()
        {
            return Ok(Domain.GetPersonWithMostFriends());
        }

        [ODataRoute("GetPeopleWithFriendsAtLeast(n={n})")]
        public IHttpActionResult GetPeopleWithFriendsAtLeast(int n)
        {
            return Ok(Domain.GetPeopleWithFriendsAtLeast(n));
        }

        [HttpPut]
        [ODataRoute("People({key})/LastName")]
        public IHttpActionResult UpdatePersonLastName([FromODataUri]int key, [FromBody]string name)
        {
            var entity = DbContext.People.Find(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.LastName = name;

            try
            {
                DbContext.SaveChanges();
            }
            catch (Exception e)
            {
                if (!PeopleExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw e;
                }
            }
            return Ok(name);
        }

        [HttpGet]
        [ODataRoute("People({key})/Trips/$ref")]
        public IHttpActionResult GetRefToTripsFromPeople([FromODataUri]int key)
        {
            var entity = DbContext.People.Find(key);
            if (entity == null)
            {
                return NotFound();
            }
            var trips = DbContext.Trips.Where(t => t.PersonId == key);
            var serviceRootUri = GetServiceRootUri();
            IList<Uri> uris = new List<Uri>();
            foreach (var trip in trips)
            {
                uris.Add(new Uri(string.Format("{0}/Trips({1})", serviceRootUri, trip.TripId)));
            }
            return Ok(uris);
        }

        [HttpGet]
        [ODataRoute("People({key})/Trips({key2})/$ref")]
        public IHttpActionResult GetRefToTripsFromPeople([FromODataUri]int key, [FromODataUri]int key2)
        {
            var entity = DbContext.People.Find(key);
            if (entity == null)
            {
                return NotFound();
            }
            var trips = DbContext.Trips.Where(t => t.PersonId == key);
            var serviceRootUri = GetServiceRootUri();

            if (trips.All(t => t.TripId != key2))
            {
                return NotFound();
            }

            return Ok(new Uri(string.Format("{0}/Trips({1})", serviceRootUri, key2)));
        }

        [HttpPost]
        [ODataRoute("People({key})/Trips")]
        public IHttpActionResult PostToTripsFromPeople([FromODataUri]int key, Trip trip)
        {
            var entity = DbContext.People.Find(key);
            if (entity == null)
            {
                return NotFound();
            }
            if (entity.PersonId != key)
            {
                return BadRequest();
            }
            DbContext.Trips.Add(trip);
            DbContext.SaveChanges();
            return Created(trip);
        }

        [HttpPost]
        [ODataRoute("People({key})/Trips/$ref")]
        public IHttpActionResult CreateRefForTripsToPeople([FromODataUri]int key, [FromBody] Uri link)
        {
            var entity = DbContext.People.Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            var relatedKey = Helpers.GetKeyFromUri<int>(Request, link);
            var trip = DbContext.Trips.SingleOrDefault(t => t.TripId == relatedKey);
            if (trip == null)
            {
                return NotFound();
            }

            trip.PersonId = key;
            DbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("People({key})/Trips({relatedKey})/$ref")]
        public IHttpActionResult DeleteRefToTripsFromPeople([FromODataUri]int key, [FromODataUri]int relatedKey)
        {
            var entity = DbContext.People.Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            var trip = DbContext.Trips.SingleOrDefault(t => t.TripId == relatedKey && t.PersonId == key);
            if (trip == null)
            {
                return NotFound();
            }

            trip.PersonId = null;
            DbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ODataRoute("Flights({key})/Airline/$ref")]
        public IHttpActionResult GetRefToAirlineFromFlight([FromODataUri]int key)
        {
            var entity = DbContext.Flights.Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            if (entity.AirlineId == null)
            {
                return NotFound();
            }

            var serviceRootUri = GetServiceRootUri();
            var uri = new Uri(string.Format("{0}/Airlines('{1}')", serviceRootUri, entity.AirlineId));
            return Ok(uri);
        }

        [HttpPut]
        [ODataRoute("Flights({key})/Airline/$ref")]
        public IHttpActionResult UpdateRefToAirLineFromFlight([FromODataUri] int key, [FromBody] Uri link)
        {
            var entity = DbContext.Flights.Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            var relatedKey = Helpers.GetKeyFromUri<string>(Request, link);
            var aireLine = DbContext.Airlines
                .SingleOrDefault(t => t.AirlineCode.Equals(relatedKey, StringComparison.OrdinalIgnoreCase));
            if (aireLine == null)
            {
                return NotFound();
            }

            entity.Airline = aireLine;
            DbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("Flights({key})/Airline/$ref")]
        public IHttpActionResult DeleteRefToAirLineFromFlight([FromODataUri] int key)
        {
            var entity = DbContext.Flights.Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            entity.AirlineId = null;
            DbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("Me")]
        public IHttpActionResult Me()
        {
            return Ok(DbContext.People.Find(1));
        }
    }
}