
using System.Net;
using mDNS = Redgate.Net.mDNS;

class Dig {
    public static void Main( string [] args ) {
        mDNS.Resolver resolver = new mDNS.Resolver();
	IPAddress[] addresses = resolver.GetA( args[0] );
	foreach ( IPAddress address in addresses ) {
	    System.Console.WriteLine( "{0}\t{1}", address.ToString(), args[0] );
	}
    }
}
