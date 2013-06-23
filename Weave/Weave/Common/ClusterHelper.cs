using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels.StartHub;
using Windows.Storage;

namespace Weave.Common
{
    public static class ClusterHelper
    {
        private const String ClusterContainerKey = "Clusters";
        private const String ClusterOrderKey = "ClusterOrder";
        private const String ClusterTypeKey = "Type";
        private const String ClusterHeaderKey = "Header";

        private const string FeedIdKey = "SearchTerm";

        private const String CategoryKey = "Category";

        public static event Action<StartClusterViewModel> ClusterRemoved;

        private enum ClusterType
        {
            Feed,
            Category
        }

        public static void test()
        {
            ApplicationDataContainer container = GetClusterContainer();
            container.Values.Remove(FeedIdKey);
        }

        /// <summary>
        /// Gets the cluster container to store/retrieve clusters.
        /// </summary>
        /// <returns>The cluster container.</returns>
        private static ApplicationDataContainer GetClusterContainer()
        {
            ApplicationDataContainer container = UserHelper.Instance.GetUserContainer(true);
            return container.CreateContainer(ClusterContainerKey, ApplicationDataCreateDisposition.Always);
        }

        private static List<String> GetClusterOrder()
        {
            String order = null;
            ApplicationDataContainer container = UserHelper.Instance.GetUserContainer(true);
            if (container.Values.ContainsKey(ClusterOrderKey))
            {
                order = (String)container.Values[ClusterOrderKey];
            }
            return order == null ? null : new List<String>(order.Split(','));
        }

        public static void UpdateClusterOrder(List<String> clusterOrder)
        {
            ApplicationDataContainer container = UserHelper.Instance.GetUserContainer(true);
            if (clusterOrder != null && clusterOrder.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (String str in clusterOrder)
                {
                    sb.Append(str + ",");
                }
                String order = sb.ToString().TrimEnd(',');
                container.Values[ClusterOrderKey] = order;
            }
            else container.Values.Remove(ClusterOrderKey);
        }

        /// <summary>
        /// Gets the stored clusters.
        /// </summary>
        /// <returns>The list of stored clusters.</returns>
        public static async Task<List<StartClusterViewModel>> GetStoredClusters()
        {
            List<StartClusterViewModel> clusters = new List<StartClusterViewModel>();
            Dictionary<String, StartClusterViewModel> unorderedClusters = new Dictionary<String, StartClusterViewModel>();
            ApplicationDataContainer clusterContainer = GetClusterContainer();
            if (clusterContainer.Values.Count > 0)
            {
                await Task.Run(() =>
                {
                    ApplicationDataCompositeValue compositeValue;
                    ClusterType clusterType;
                    foreach (KeyValuePair<String, object> kv in clusterContainer.Values)
                    {
                        compositeValue = kv.Value as ApplicationDataCompositeValue;
                        if (compositeValue != null && compositeValue.ContainsKey(ClusterTypeKey) && Enum.TryParse<ClusterType>((String)compositeValue[ClusterTypeKey], out clusterType))
                        {
                            StartClusterViewModel vm = new StartClusterViewModel();
                            vm.Header = (String)compositeValue[ClusterHeaderKey];
                            switch (clusterType)
                            {
                                case ClusterType.Feed:
                                    vm.FeedId = new Guid((String)compositeValue[FeedIdKey]);
                                    break;
                                case ClusterType.Category:
                                    vm.Category = (String)compositeValue[CategoryKey];
                                    break;
                                default:
                                    break;
                            }
                            unorderedClusters.Add(kv.Key, vm);
                        }
                    }

                    List<String> clusterOrder = GetClusterOrder();
                    if (clusterOrder != null && clusterOrder.Count > 0)
                    {
                        foreach (String str in clusterOrder)
                        {
                            if (unorderedClusters.ContainsKey(str))
                            {
                                clusters.Add(unorderedClusters[str]);
                                unorderedClusters.Remove(str);
                            }
                        }

                        foreach (StartClusterViewModel c in unorderedClusters.Values) clusters.Add(c);
                    }
                    else
                    {
                        clusters = new List<StartClusterViewModel>(unorderedClusters.Values);
                    }
                });
            }

            return clusters;
        }

        public static void StoreFeedCluster(StartClusterViewModel cluster)
        {
            if (cluster != null && cluster.FeedId != null)
            {
                ApplicationDataContainer clusterContainer = GetClusterContainer();
                ApplicationDataCompositeValue compositeValue = new ApplicationDataCompositeValue();
                compositeValue.Add(ClusterHeaderKey, cluster.Header);
                String feedIdString = cluster.FeedId.ToString();
                compositeValue.Add(FeedIdKey, feedIdString);
                compositeValue.Add(ClusterTypeKey, ClusterType.Feed.ToString());
                clusterContainer.Values[feedIdString] = compositeValue;
            }
        }

        public static void StoreCategoryCluster(StartClusterViewModel cluster)
        {
            if (cluster != null && !string.IsNullOrEmpty(cluster.Category))
            {
                ApplicationDataContainer clusterContainer = GetClusterContainer();
                ApplicationDataCompositeValue compositeValue = new ApplicationDataCompositeValue();
                compositeValue.Add(ClusterHeaderKey, cluster.Header);
                compositeValue.Add(CategoryKey, cluster.Category);
                compositeValue.Add(ClusterTypeKey, ClusterType.Category.ToString());
                clusterContainer.Values[cluster.Category] = compositeValue;
            }
        }

        public static void RemoveCluster(StartClusterViewModel cluster)
        {
            ApplicationDataContainer clusterContainer = GetClusterContainer();
            bool removed = true;
            if (!String.IsNullOrEmpty(cluster.Category)) clusterContainer.Values.Remove(cluster.Category);
            else if (cluster.FeedId != null) clusterContainer.Values.Remove(cluster.FeedId.Value.ToString());
            else removed = false;

            if (removed && ClusterRemoved != null) ClusterRemoved(cluster);
        }

    } // end of class
}
