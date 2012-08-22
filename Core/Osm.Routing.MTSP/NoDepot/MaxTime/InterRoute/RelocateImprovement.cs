﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.Math.VRP.Core.Routes;
using Tools.Math.VRP.Core.BestPlacement;

namespace Osm.Routing.Core.VRP.NoDepot.MaxTime.InterRoute
{
    /// <summary>
    /// Relocate heurstic.
    /// </summary>
    public class RelocateImprovement : IInterRouteImprovement
    {
        /// <summary>
        /// Tries to improve the existing routes by re-inserting a customer from one route into another.
        /// </summary>
        /// <param name="problem"></param>
        /// <param name="route1"></param>
        /// <param name="route2"></param>
        /// <param name="difference"></param>
        /// <returns></returns>
        public bool Improve(MaxTimeProblem problem, IRoute route1, IRoute route2, out float difference)
        {
            if (this.RelocateFromTo(problem, route1, route2, out difference))
            {
                return true;
            }
            if (this.RelocateFromTo(problem, route2, route1, out difference))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries a relocation of the customers in route1 to route2.
        /// </summary>
        /// <param name="problem"></param>
        /// <param name="route1"></param>
        /// <param name="route2"></param>
        /// <param name="difference"></param>
        /// <returns></returns>
        private bool RelocateFromTo(MaxTimeProblem problem, IRoute route1, IRoute route2, out float difference)
        {
            int previous = -1;
            int current = -1;
            foreach (int next in route1)
            {
                if (previous >= 0 && current >= 0)
                { // consider the next customer.
                    if (this.ConsiderCustomer(problem, route2, previous, current, next, out difference))
                    {
                        route1.Remove(current);
                        break;
                    }
                }

                previous = current;
                current = next;
            }
            difference = 0;
            return false;
        }

        /// <summary>
        /// Considers one customer for relocation.
        /// </summary>
        /// <param name="problem"></param>
        /// <param name="route"></param>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private bool ConsiderCustomer(MaxTimeProblem problem, IRoute route, int previous, int current, int next, out float difference)
        {
            // calculate the removal gain of the customer.
            float removal_gain = problem.WeightMatrix[previous][current] + problem.WeightMatrix[current][next]
                - problem.WeightMatrix[previous][next];
            if (removal_gain > 0)
            {
                // try and place the customer in the next route.
                CheapestInsertionResult result = 
                    CheapestInsertionHelper.CalculateBestPlacement(problem, route, current);
                if (result.Increase < removal_gain)
                { // there is a gain in relocating this customer.
                    difference = result.Increase - removal_gain;

                    route.Insert(result.CustomerBefore, result.Customer, result.CustomerAfter);
                }
            }
            difference = 0;
            return false;
        }
    }
}
