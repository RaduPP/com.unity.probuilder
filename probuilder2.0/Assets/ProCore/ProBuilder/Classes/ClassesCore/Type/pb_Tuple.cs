
namespace ProBuilder2.Common
{
	public class pb_Tuple<K, V>
	{
		public K Item1;
		public V Item2;

		public pb_Tuple() {}

		public pb_Tuple(K item1, V item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}
	}
}