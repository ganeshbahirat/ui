﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.PreProcessing;
using OsmSharp.Routing.CH.PreProcessing.Ordering.LimitedLevelOrdering;
using OsmSharp.Routing.CH.PreProcessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Streams.Graphs;
using OsmSharp.Routing.CH.PreProcessing.Ordering;
using OsmSharp.Routing.Osm.Interpreter;

namespace OsmSharp.Test.Unittests.Routing.CH
{
    /// <summary>
    /// Tests the CH Sparse routing against a reference implementation.
    /// </summary>
    [TestFixture]
    public class CHEdgeDifferenceComparisonTests : RoutingComparisonTests
    {
        /// <summary>
        /// Holds the data.
        /// </summary>
        private Dictionary<string, DynamicGraphRouterDataSource<CHEdgeData>> _data = null;

        /// <summary>
        /// Returns a new router.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedName"></param>
        /// <returns></returns>
        public override Router BuildRouter(IOsmRoutingInterpreter interpreter, string embeddedName)
        {
            if (_data == null)
            {
                _data = new Dictionary<string, DynamicGraphRouterDataSource<CHEdgeData>>();
            }
            DynamicGraphRouterDataSource<CHEdgeData> data = null;
            if (!_data.TryGetValue(embeddedName, out data))
            {
                var tagsIndex = new SimpleTagsIndex();

                // do the data processing.
                data =
                    new DynamicGraphRouterDataSource<CHEdgeData>(tagsIndex);
                var targetData = new CHEdgeGraphOsmStreamTarget(
                    data, interpreter, data.TagsIndex, Vehicle.Car);
                var dataProcessorSource = new XmlOsmStreamSource(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(
                    "OsmSharp.Test.Unittests.{0}", embeddedName)));
                var sorter = new OsmStreamFilterSort();
                sorter.RegisterSource(dataProcessorSource);
                targetData.RegisterSource(sorter);
                targetData.Pull();

                // do the pre-processing part.
                var witnessCalculator = new DykstraWitnessCalculator(data);
                var preProcessor = new CHPreProcessor(data,
                    new EdgeDifference(data, witnessCalculator), witnessCalculator);
                preProcessor.Start();

                _data[embeddedName] = data;
            }
            return Router.CreateCHFrom(data, new CHRouter(
                data), interpreter);
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceAgainstReference()
        {
            this.TestCompareAll("test_network.osm");
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceOneWayAgainstReference()
        {
            this.TestCompareAll("test_network_oneway.osm");
        }

        ///// <summary>
        ///// Compares all routes possible against a reference implementation.
        ///// </summary>
        //[Test]
        //public void TestCHEdgeDifferenceRegression2()
        //{
        //    this.TestCompareAll("test_routing_regression2.osm");
        //}

        ///// <summary>
        ///// Compares all routes possible against a reference implementation.
        ///// </summary>
        //[Test]
        //public void TestCHEdgeDifferenceBig()
        //{
        //    this.TestCompareAll("test_network_big.osm");
        //}

        ///// <summary>
        ///// Compares all routes possible against a reference implementation.
        ///// </summary>
        //[Test]
        //public void TestCHEdgeDifferenceAgainstReferenceRealNetwork()
        //{
        //    this.TestCompareAll("test_network_real1.osm");
        //}
    }
}