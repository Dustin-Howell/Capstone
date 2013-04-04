﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FireAndIce.ViewModels;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Collections;

namespace FireAndIce
{
    public class AppBootStrapper : Bootstrapper<AppViewModel>
    {
        private CompositionContainer _container;
 
        protected override void Configure()
        {
            _container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
 
            CompositionBatch batch = new CompositionBatch();
 
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(_container);
 
            _container.Compose(batch);
        }
 
        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract);
 
            if (exports.Count() > 0)
            {
                return exports.First();
            }
 
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }
    }
}
