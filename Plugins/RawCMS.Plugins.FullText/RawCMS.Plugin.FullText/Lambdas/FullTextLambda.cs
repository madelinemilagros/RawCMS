﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using Newtonsoft.Json.Linq;
using RawCMS.Library.Core;
using RawCMS.Plugins.FullText.Core;
using System.Collections.Generic;
using System.Linq;

namespace RawCMS.Plugins.FullText.Lambdas
{
    public abstract class BaseFullTextLambda : DataProcessLambda
    {
        protected readonly FullTextService fullTextService;
        protected readonly FullTextUtilityService helper;

        public BaseFullTextLambda(FullTextService fullTextService, FullTextUtilityService helper)
        {
            this.fullTextService = fullTextService;
            this.helper = helper;
        }
    }

    public class DeleteFullTextLambda : BaseFullTextLambda
    {
        public override string Name => "FullTextMapping";

        public override string Description => this.Name;

        public override SavePipelineStage Stage => SavePipelineStage.PostSave;

        public override DataOperation Operation => DataOperation.Delete;

        public DeleteFullTextLambda(FullTextService fullTextService, FullTextUtilityService helper) : base(fullTextService, helper)
        {
        }

        public override void Execute(string collection, ref JObject item, ref Dictionary<string, object> dataContext)
        {
            var filter = helper.GetFilter(collection);
            if (filter == null) return;

            var id = item["_id"];
            if (id == null) return;

            var index = helper.GetIndexName(collection);
            this.fullTextService.DeleteDocument(index, id.ToString());
        }
    }

    public class FullTextLambda : BaseFullTextLambda
    {
        public override string Name => "FullTextMapping";

        public override string Description => this.Name;

        public override SavePipelineStage Stage => SavePipelineStage.PostSave;

        public override DataOperation Operation => DataOperation.Write;

        protected readonly FullTextService fullTextService;
        protected readonly FullTextUtilityService helper;

        public FullTextLambda(FullTextService fullTextService, FullTextUtilityService helper) : base(fullTextService, helper)
        {
        }

        public override void Execute(string collection, ref JObject item, ref Dictionary<string, object> dataContext)
        {
            var filter = helper.GetFilter(collection);
            if (filter == null) return;

            JObject searchDocument = new JObject();

            var list = new List<string>()
                {
                    "_id" //id is alway neededs
                };

            //if empty add all
            if (filter.IncludedField == null || filter.IncludedField.Count == 0)
            {
                list.AddRange(item.Properties().Select(p => p.Name).Distinct().ToList());
            }

            foreach (var field in filter.IncludedField)
            {
                searchDocument[field] = item[field];
            }

            this.fullTextService.AddDocumentRaw(this.helper.GetIndexName(collection), searchDocument);
        }
    }
}