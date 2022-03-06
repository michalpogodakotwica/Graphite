﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Runtime;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;

namespace Graphite.Editor.CopyPasteHandler
{
	public class DefaultCopyPasteHandler : ICopyPasteHandler
	{
		private static readonly JsonSerializerSettings Settings = new()
		{
			ContractResolver = new UnitySerializationResemblingJsonContractResolver(),
			TypeNameHandling = TypeNameHandling.Objects
		};

		public string SerializeGraphElementsCallback(GraphDrawer.GraphDrawer graphDrawer, IEnumerable<GraphElement> elements)
		{
			var insideNodes = elements.Where(c => c is NodeDrawer).Cast<NodeDrawer>().Select(n => n.Content).ToList();
			
			// outside connections are not stripped before serialization - remember to strip them when deserializing
			var graphData = new DefaultCopyPasteGraphData
			{
				Nodes = insideNodes
			};

			var data = JsonConvert.SerializeObject(graphData, Formatting.Indented, Settings);
			return data;
		}
		
		public bool CanPasteSerializedDataCallback(GraphDrawer.GraphDrawer graphDrawer, string data)
		{
			try
			{
				JsonConvert.DeserializeObject<DefaultCopyPasteGraphData>(data, Settings);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void UnserializeAndPasteCallback(GraphDrawer.GraphDrawer graphDrawer, string operationName, string data)
		{
			graphDrawer.ClearSelection();

			var copyPasteGraphData = JsonConvert.DeserializeObject<DefaultCopyPasteGraphData>(data, Settings);
			
			if (copyPasteGraphData == null)
				return;
			
			var nodes = copyPasteGraphData?.Nodes;

			var internalInputPorts = nodes.SelectMany(GetAllInputs).ToArray();
			var internalOutputPorts =  nodes.SelectMany(GetAllOutputs).ToArray();

			// stripping outside connections
			foreach (var inputPort in internalInputPorts)
			foreach (var connection in inputPort.Connections.ToArray())
				if(!internalOutputPorts.Contains(connection))
					inputPort.Disconnect(connection);

			graphDrawer.ModifyWithUndo(() =>
			{
				graphDrawer.AddNodes(nodes);
			});
		}

		private static IEnumerable<IInput> GetAllInputs(INode node)
		{
			return node.GetType()
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(f => typeof(IInput).IsAssignableFrom(f.FieldType))
				.Select(f => (IInput)f.GetValue(node));
		}
		
		private static IEnumerable<IOutput> GetAllOutputs(INode node)
		{
			return node.GetType()
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(f => typeof(IOutput).IsAssignableFrom(f.FieldType))
				.Select(f => (IOutput)f.GetValue(node));
		}
	}
}