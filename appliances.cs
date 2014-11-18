
using System.Net;
using mDNS = Redgate.Net.mDNS;

class Appliances {
    public static void Main( string [] args ) {
	mDNS.Resolver resolver = new mDNS.Resolver();
	IPEndPoint[] endpoints = resolver.ResolveServiceName( "_appliance._tcp.local" );
	if ( endpoints.Length == 0 ) {
	    System.Console.WriteLine( "No appliance servers available" );
	    return;
	}
	foreach ( IPEndPoint endpoint in endpoints ) {
	    System.Console.WriteLine( "Appliances at: " + endpoint );
	}
    }
}
