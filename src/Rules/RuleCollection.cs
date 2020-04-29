﻿using IdFix.Settings;
using System;
using System.DirectoryServices.Protocols;

namespace IdFix.Rules
{
    #region RuleCollectionResult

    public class RuleCollectionResult
    {
        /// <summary>
        /// Total number of entities found and processed (skipped, errors, success)
        /// </summary>
        public long TotalFound { get; set; }

        /// <summary>
        /// The number of entities skipped
        /// </summary>
        public long TotalSkips { get; set; }

        /// <summary>
        /// The number of entities processed through rules (not skipped)
        /// </summary>
        public long TotalProcessed { get; set; }

        /// <summary>
        /// The number of entities found to have errors (not the total number of errors as an entity could contain multiple errors)
        /// </summary>
        public long TotalErrors { get; set; }

        /// <summary>
        /// The number of entities found to have duplicates
        /// </summary>
        public long TotalDuplicates { get; set; }

        /// <summary>
        /// Records the time spent running the rules against the connection
        /// </summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary>
        /// The set of all errors found when running rules against the entities
        /// </summary>
        public ComposedRuleResult[] Errors { get; set; }
    }

    #endregion

    abstract class RuleCollection
    {
        protected RuleCollection(string distinguishedName, int pageSize = 1000)
        {
            this.DistinguishedName = distinguishedName;
            this.PageSize = pageSize;
        }

        public event OnStatusUpdateDelegate OnStatusUpdate;

        protected string DistinguishedName { get; private set; }
        protected int PageSize { get; private set; }

        public abstract string[] AttributesToQuery { get; }
        public abstract bool Skip(SearchResultEntry entry);
        public abstract IComposedRule[] Rules { get; }

        #region CreateSearchRequest

        /// <summary>
        /// Creates the search request for this rule collection
        /// </summary>
        /// <returns>Configured search request</returns>
        public virtual SearchRequest CreateSearchRequest(bool includePaging = true)
        {
            var searchRequest = new SearchRequest(
                this.DistinguishedName,
                SettingsManager.Instance.Filter,
                SearchScope.Subtree,
                this.AttributesToQuery);

            if (includePaging)
            {
                searchRequest.Controls.Add(new PageResultRequestControl(this.PageSize));
            }

            return searchRequest;
        }

        #endregion

        #region InvokeStatus

        /// <summary>
        /// Invokes the OnStatusUpdate event with the supplied message
        /// </summary>
        /// <param name="message">Message to send</param>
        protected virtual void InvokeStatus(string message)
        {
            this.OnStatusUpdate?.Invoke(message);
        }

        #endregion
    }
}
