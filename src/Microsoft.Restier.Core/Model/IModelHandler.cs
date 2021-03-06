﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace Microsoft.Restier.Core.Model
{
    /// <summary>
    /// Represents a hook point that implements a model flow.
    /// </summary>
    /// <remarks>
    /// This is a singleton hook point with a default implementation.
    /// </remarks>
    public interface IModelHandler
    {
        /// <summary>
        /// Asynchronously executes the model flow.
        /// </summary>
        /// <param name="context">
        /// The model context.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous
        /// operation whose result is a domain model.
        /// </returns>
        Task<IEdmModel> GetModelAsync(
            ModelContext context,
            CancellationToken cancellationToken);
    }
}
