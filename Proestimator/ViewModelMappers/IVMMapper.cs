﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModelMappers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVMMapper<T>
    {
        T Map(MappingConfiguration mappingConfiguration);
    }
}