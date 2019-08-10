﻿using Mastonet;
using Mastonet.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tooter.Enums;
using Tooter.Helpers;
using Tooter.Model;
using Tooter.Services;

namespace Tooter.ViewModel
{
    class FederatedViewModel : TimelineViewModelBase
    {
        public override string ViewTitle { get; protected set; } = "Federated Timeline";

        public override event EventHandler TootsAdded;

       

        

        internal async override Task AddNewerContentToFeed()
        {
            Mastonet.ArrayOptions options = new Mastonet.ArrayOptions();

            options.MinId = previousPageMinId;
            options.SinceId = previousPageSinceId;

            var newerContent = await ClientHelper.Client.GetPublicTimeline(options);

            previousPageMinId = newerContent.PreviousPageMinId;
            previousPageSinceId = newerContent.PreviousPageSinceId;

            tootTimelineData.InsertRange(0, newerContent);

            for (int i = newerContent.Count - 1; i > -1; i--)
            {
                TootTimelineCollection.Insert(0, newerContent[i]);
            }

            TootsAdded?.Invoke(null, EventArgs.Empty);
        }

        internal async override Task AddOlderContentToFeed()
        {
            var options = new ArrayOptions();
            options.MaxId = nextPageMaxId;
            var olderContent = await ClientHelper.Client.GetPublicTimeline(options);
            nextPageMaxId = olderContent.NextPageMaxId;

            tootTimelineData.AddRange(olderContent);
            foreach (var item in olderContent)
            {
                TootTimelineCollection.Add(item);
            }

            TootsAdded?.Invoke(null, EventArgs.Empty);
        }

        internal override void CacheTimeline(Status currentTopVisibleStatus)
        {
            var timelineSettings = new TimelineSettings(nextPageMaxId, previousPageMinId, previousPageSinceId, TimelineType.Federated);
            CacheService.CacheTimeline(tootTimelineData, currentTopVisibleStatus, timelineSettings);
        }

        internal async override Task LoadFeedAsync()
        {
            base.tootTimelineData = await ClientHelper.Client.GetPublicTimeline();
            nextPageMaxId = tootTimelineData.NextPageMaxId;
            previousPageMinId = tootTimelineData.PreviousPageMinId;
            previousPageSinceId = tootTimelineData.PreviousPageSinceId;

            TootTimelineCollection = new System.Collections.ObjectModel.ObservableCollection<Mastonet.Entities.Status>(tootTimelineData);
        }
    }
}
