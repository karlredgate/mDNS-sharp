
using System.Net;
using mDNS = Redgate.Net.mDNS;

class Services {
    public static void Main( string [] args ) {
	mDNS.Resolver resolver = new mDNS.Resolver();
	IPEndPoint[] endpoints = resolver.ResolveServiceName( "_" + args[0] + "._tcp.local" );
	if ( endpoints.Length == 0 ) {
	    System.Console.WriteLine( "No {0} servers available", args[0] );
	    return;
	}
	foreach ( IPEndPoint endpoint in endpoints ) {
	    System.Console.WriteLine( "Services at: " + endpoint );
	}
    }
}
