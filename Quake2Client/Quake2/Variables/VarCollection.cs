using System;
using System.Collections;
using System.Collections.Generic;
//using System.Dynamic;
using Quake2.Client.Events;
using Quake2.Extensions;

namespace Quake2.Variables
{
	public class VarCollection : ICollection<Variable>, IDictionary<string, Variable>
	{
		public delegate void ValueChanged(VarCollection collection, VariableValueChangedEventArgs args);
		public event ValueChanged OnValueChanged;

		void FireValueChanged(object sender, ValueChangedEventArgs<object> args)
		{
			if (OnValueChanged != null)
				OnValueChanged(this, new VariableValueChangedEventArgs((Variable)sender, args.OldValue, args.NewValue));
		}

		Dictionary<string, Variable> _vars;

		public VarCollection()
		{
			_vars = new Dictionary<string, Variable>();
		}

		public Variable GetVar(string name)
		{
			if (_vars.ContainsKey(name))
				return _vars[name];

			return null;
		}

		/// <summary>
		/// Adiciona, ou sobrescreve caso já exista.
		/// </summary>
		/// <param name="item"></param>
		public void UseVar(Variable item)
		{
			Remove(item.Name);
			Add(item);
		}

		/// <summary>
		/// Adiciona, ou sobrescreve caso já exista.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public virtual void SetVar(string name, object value)
		{
			if (value is Variable)
				throw new InvalidOperationException("Call UseVar(Variable item) instead.");

			if (!_vars.ContainsKey(name))
				Add(name, value);
			else
				_vars[name].Value = value;
		}

		#region Dynamic

		private Variable GetVarByLooseName(string name)
		{
			if (_vars.ContainsKey(name))
				return _vars[name];

			if (_vars.ContainsKey(name.ToLower()))
				return _vars[name.ToLower()];

			if (!_vars.ContainsKey(name.ToLowerId()))
				return null;

			return _vars[name.ToLowerId()];
		}

		public object GetMember(string name)
		{
			return GetVarByLooseName(name);
		}

		public bool SetMember(string name, object value)
		{
			Variable existingVar = GetVarByLooseName(name);

			if (value is Variable)
			{
				if (existingVar != null)
					SetVar(existingVar.Name, (Variable)value);
				else
					Add((Variable)value);
			}
			else
			{
				if (existingVar != null)
					existingVar.Value = value;
				else
					Add(name.ToLowerId(), value);
			}

			return true;
		}

		#endregion

		#region ICollection

		public void Add(string name, object var)
		{
			if (var is Variable)
			{
				_vars.Add(name, (Variable)var);
				((Variable)var).OnValueChanged -= FireValueChanged;
				((Variable)var).OnValueChanged += FireValueChanged;
			}
			else
				Add(new Variable(name, var));
		}

		public virtual void Add(Variable item)
		{
			_vars.Add(item.Name, item);
			item.OnValueChanged -= FireValueChanged;
			item.OnValueChanged += FireValueChanged;
		}

		public void Clear()
		{
			foreach (Variable var in _vars.Values)
				var.OnValueChanged -= FireValueChanged;
			_vars.Clear();
		}

		public bool Contains(string itemName)
		{
			return _vars.ContainsKey(itemName);
		}

		public bool Contains(Variable item)
		{
			return _vars.ContainsKey(item.Name) && _vars[item.Name] == item;
		}

		public void CopyTo(Variable[] array, int arrayIndex)
		{
			_vars.Values.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _vars.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(string itemName)
		{
			if (_vars.ContainsKey(itemName))
				_vars[itemName].OnValueChanged -= FireValueChanged;
			return _vars.Remove(itemName);
		}

		public bool Remove(Variable item)
		{
			if (Contains(item))
				return Remove(item.Name);
			return false;
		}

		IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator()
		{
			return _vars.Values.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return _vars.Values.GetEnumerator();
		}
		#endregion

		public void Add(string key, Variable value)
		{
			_vars.Add(key, value);
			value.OnValueChanged -= FireValueChanged;
			value.OnValueChanged += FireValueChanged;
		}

		public bool ContainsKey(string key)
		{
			return Contains(key);
		}

		public ICollection<string> Keys
		{
			get { return _vars.Keys; }
		}

		public bool TryGetValue(string key, out Variable value)
		{
			return _vars.TryGetValue(key, out value);
		}

		public ICollection<Variable> Values
		{
			get { return _vars.Values; }
		}

		public Variable this[string key]
		{
			get { return _vars[key]; }
			set { _vars[key] = value; }
		}

		public void Add(KeyValuePair<string, Variable> item)
		{
			Add(item.Key, item.Value);
		}

		public bool Contains(KeyValuePair<string, Variable> item)
		{
			return _vars.ContainsKey(item.Key) && _vars[item.Key] == item.Value;
		}

		public void CopyTo(KeyValuePair<string, Variable>[] array, int arrayIndex)
		{
			foreach (var kv in _vars)
				array[arrayIndex++] = kv;
		}

		public bool Remove(KeyValuePair<string, Variable> item)
		{
			if (Contains(item))
				return _vars.Remove(item.Key);
			return false;
		}

		IEnumerator<KeyValuePair<string, Variable>> IEnumerable<KeyValuePair<string, Variable>>.GetEnumerator()
		{
			return _vars.GetEnumerator();
		}
	}
}
