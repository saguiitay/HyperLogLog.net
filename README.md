# [DEPRECATED]

This project is superseded by https://github.com/saguiitay/CardinalityEstimation, and is no longer maintained.


HyperLogLog.net
===============

HyperLogLog.net is a C# implementation of HyperLogLog - a near-optimal cardinality estimation algorithm.

HLL was conceived of by Flajolet et. al. in the phenomenal paper [HyperLogLog: the analysis of a near-optimal cardinality estimation algorithm](http://algo.inria.fr/flajolet/Publications/FlFuGaMe07.pdf).

A visual demonstration of HyperLogLog is available [here](http://content.research.neustar.biz/blog/hll.html) along with a [basic explanation](http://research.neustar.biz/2012/10/25/sketch-of-the-day-hyperloglog-cornerstone-of-a-big-data-infrastructure/) about it.

## Contact

You can contact me on twitter [@saguiitay](https://twitter.com/saguiitay).

## NuGet

HyperLogLog.net is available as a [NuGet package](https://www.nuget.org/packages/HyperLogLog.net/)

## Features

* Support custom `HashAlgorithm`s. (Default is MD5, but I strongly recommend using Murmur3 from [murmurhash](https://www.nuget.org/packages/murmurhash/)
* Support custom byte[] converters. (Default handles value types, and calls `ToString()` on all other types)
* Support serialization into/from byte[] for network transfer
* Allows merging multiple HyperLogLog objects into a single HyperLogLog. Useful if you are handling a high volume of data on multiple machines, and needs to aggregate the data for a "final" number.

## Release Notes

+ **1.0.0**   Initial release.

# Usage

``` csharp
var hashAlgorithm = Murmur.MurmurHash.Create128(managed: false); // Usign a custom HashAlgorithm. Default is MD5
var hyperLogLogSerializer = new HyperLogLogSerializer(); // Easy serialization/deserialization of the counter
const int count = 1000000;

// Check %error at different accuracy level (more accuracy requires more memory)
for (int b = 4; b <= 16; b++)
{
  var hll = new HyperLogLog.net.HyperLogLog(hashAlgorithm, b);

  for (int i = 1; i < count + 1; i++)
  {
    // Log appearances of "random" data stream 
    hll.LogData(Guid.NewGuid());
  }

  // Serialize the HyperLogLog to a byte[] (useful for sending over the network)
  var outputStream = new MemoryStream();
  hyperLogLogSerializer.SerializeTo(hll, outputStream);

  // Deserialize back to a HyperLogLog
  var inputStream = new MemoryStream(outputStream.ToArray());
  var hllDeserialized = hyperLogLogSerializer.DeserializeTo(inputStream, hashAlgorithm);

  // Ensure serialization/deserialization didn't ruin our data
  var cardinality = hll.GetCount();
  var cardinalityDeserialized = hllDeserialized.GetCount();

  if (cardinality != cardinalityDeserialized)
    throw new Exception("Serialization/Deserialization ruined the data!");

  // Display Actual data seeing, Recorded (Estimated) data, and error %
  var error = 1 - (cardinality/(double) (3 * count));
  Console.WriteLine("B: {0}\tActual: {1}\tEstimated: {2}\t%Error: {3}", b, count, cardinality, error);
}
```
