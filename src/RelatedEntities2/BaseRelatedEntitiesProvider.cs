using System;
using System.Collections.Generic;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.DataStore.Document.Models;
using CluedIn.RelatedEntities;
using Microsoft.Extensions.Logging;

namespace CluedIn.RelatedEntities2
{
    public abstract class BaseRelatedEntitiesProvider : IRelatedEntitiesProvider
    {
        public readonly EntityType EntityType;
        public readonly string OriginName;

        public BaseRelatedEntitiesProvider(EntityType entityType, string originName = null)
        {
            EntityType = entityType;
            OriginName = originName;
        }

        public abstract IEnumerable<SuggestedSearch> SuggestedSearches(Guid id);
        public RelatedEntitiesHelper RelatedEntitiesHelper { get; set; }

        public IEnumerable<SuggestedSearch> GetRelatedEntitiesSearches(ExecutionContext context, Entity entity)
        {
            var Log = context.Log;

            if (entity.Type != EntityType || (entity.OriginEntityCode.Origin != OriginName && OriginName != null))
            {
                Log.LogInformation($"[Related Entities] {EntityType}{", " + OriginName}:  - nothing to suggest");
                return new SuggestedSearch[0];
            }

            RelatedEntitiesHelper = new RelatedEntitiesHelper(context, entity);
            var searches = new List<SuggestedSearch>();

            foreach (var suggestedSearch in SuggestedSearches(entity.Id))
            {
                try
                {
                    if (RelatedEntitiesUtility.CypherFluentQueriesCount(suggestedSearch.SearchQuery, suggestedSearch.Tokens, context) > 0)
                    {
                        Log.LogInformation($"[Related Entities] {EntityType}{", " + OriginName}: CypherFluentQueries matches - {suggestedSearch.DisplayName}");
                        searches.Add(suggestedSearch);
                    }
                    else
                        Log.LogInformation($"[Related Entities] {EntityType}{", " + OriginName}: CypherFluentQueries does not match - {suggestedSearch.DisplayName}");
                }
                catch (Exception ex)
                {
                    Log.LogCritical(ex, $"[Related Entities] {EntityType}{", " + OriginName}: Error in executing Suggested Search. Query: '{suggestedSearch.SearchQuery}' Token: '{suggestedSearch.Tokens}'");
                }
            }

            return searches;
        }
    }
}
