using Quake2.Client.Events;

namespace Quake2.Variables
{
	public class Variable
	{
		public event ValueChangedEventHandler<object> OnValueChanged;

		public Variable(string name, object value)
		{
			Name = name;
			Value = value;
			DefaultValue = value;
		}

		object _value;
		public object DefaultValue { get; private set; }

		public string Name { get; private set; }
		public object Value
		{
			get { return _value; }
			set
			{
				var oldValue = _value;
				_value = value;
				if (OnValueChanged != null && (oldValue == null || !oldValue.Equals(_value)))
					OnValueChanged(this, new VariableValueChangedEventArgs(this, oldValue, value));
			}
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		#region Implicit Conversions

		public static implicit operator bool(Variable var)
		{
			if (var.Value is bool)
				return (bool)var.Value;
			return var.Value.ToString() != "" && var.Value.ToString() != "0";
		}

		public static implicit operator string(Variable var)
		{
			return var.Value.ToString();
		}

		public static implicit operator int(Variable var)
		{
			if (var.Value is bool)
				return ((bool)var.Value) ? 1 : 0;

			if (var.Value is int)
				return (int)var.Value;
			if (var.Value == null || var.Value.ToString() == string.Empty)
				return 0;
			return int.Parse(var.Value.ToString());
		}

		public static implicit operator double(Variable var)
		{
			if (var.Value is int)
				return (double)(int)var.Value;
			return (double)var.Value;
		}

		public static implicit operator float(Variable var)
		{
			if (var.Value is int)
				return (float)(int)var.Value;
			return (float)var.Value;
		}

		#endregion
	}

	public class MappingVariable : Variable
	{
		public VariableValueChangedEventHandler Converter { get; private set; }
		public VariableValueChangedEventHandler ReverseConverter { get; private set; }

		public MappingVariable(Variable source, VariableValueChangedEventHandler converter, VariableValueChangedEventHandler reverseConverter)
			: base(source.Name, converter(null, new VariableValueChangedEventArgs(source, null, source.DefaultValue)))
		{
			Converter = converter;
			ReverseConverter = reverseConverter;

			Value = Converter(this, new VariableValueChangedEventArgs(source, null, source.Value));
			source.OnValueChanged += UpdateLocal;

			OnValueChanged += UpdateRemote;
		}

		void UpdateLocal(object sender, ValueChangedEventArgs<object> e)
		{
			OnValueChanged -= UpdateRemote;
			Value = Converter(this, new VariableValueChangedEventArgs((Variable)sender, e.OldValue, e.NewValue));
			OnValueChanged += UpdateRemote;
		}

		void UpdateRemote(object sender, ValueChangedEventArgs<object> e)
		{
			Value = ReverseConverter(this, new VariableValueChangedEventArgs(this, e.OldValue, e.NewValue));
		}
	}

	public class MapBoolToInt : MappingVariable
	{
		public MapBoolToInt(Variable source)
			: base(source, (s, e) => e.Variable ? 1 : 0, (s, e) => e.Variable > 0)
		{
		}
	}
}