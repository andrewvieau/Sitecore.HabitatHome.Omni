﻿using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.HabitatHome.Fitness.Personalization.Services
{
    public class EventSearchResultItem : SearchResultItem
    {
        [IndexField("date")]
        public DateTime Date { get; set; }

        [IndexField("latitude")]
        public float Latitude { get; set; }

        [IndexField("longitude")]
        public float Longitude { get; set; }

        [IndexField("_profilenames")]
        public string ProfileNames { get; set; }
    }

    /// <summary>
    /// Data Service responsible for fetching event items
    /// </summary>
    public class EventDataService : IEventDataService
    {
        public IEnumerable<Item> GetAll([NotNull]Database database, string[] profileNames, int take, int skip, float latitude, float longitude, out int totalSearchResults)
        {
            using (var context = GetIndex(database).CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck))
            {
                // building query
                var query = PredicateBuilder.True<EventSearchResultItem>();

                var templateQuery = PredicateBuilder.True<EventSearchResultItem>();
                templateQuery = templateQuery.And(i => i.TemplateId == Wellknown.TemplateIds.Event);

                var dateQuery = PredicateBuilder.True<EventSearchResultItem>();
                dateQuery = dateQuery.And(i => i.Date > DateTime.UtcNow);

                var profileNamesQuery = PredicateBuilder.True<EventSearchResultItem>();
                foreach(var profileName in profileNames)
                {
                    profileNamesQuery = profileNamesQuery.Or(item => item.ProfileNames.Equals(profileName));
                }

                // joining the queries
                query = query.And(templateQuery);
                query = query.And(dateQuery);
                query = query.And(profileNamesQuery);

                // getting the results
                var searchResults = context.GetQueryable<EventSearchResultItem>()
                                            .Where(query)
                                            .OrderBy(i => i.Date)
                                            .Take(take)
                                            .Skip(skip)
                                            .GetResults();

                totalSearchResults = searchResults.TotalSearchResults;
                return searchResults.Select(i => i.Document.GetItem()).ToList();
            }
        }

        private ISearchIndex GetIndex([NotNull]Database database)
        {
            return ContentSearchManager.GetIndex($"sitecore_{database.Name}_index");
        }
    }
}