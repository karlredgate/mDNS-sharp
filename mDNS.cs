
using System;                           // for Serializable
using System.Runtime.InteropServices;   // for StructLayout

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections;               // for ArrayList

namespace Redgate.Net.mDNS {

    public class Query {
        string domain;

        protected ushort QueryType { get; set; }
        public ushort QueryClass { get { return 1; } }

        public Query( string name ) {
            QueryType = 0;
            // validate name
            domain = name;
        }

        virtual public byte[] GetBytes() {
            PacketBuilder packet = new PacketBuilder();
            packet.Add( domain );
            packet.Add( QueryType );
            packet.Add( QueryClass );
            return packet.GetBytes();
        }
    }

    public class QueryA : Query {
        public QueryA( string name ) : base(name) { QueryType = 1; }
    }

    public class QueryCNAME : Query {
        public QueryCNAME( string name ) : base(name) { QueryType = 5; }
    }

    public class QueryPTR : Query {
        public QueryPTR( string name ) : base(name) { QueryType = 12; }
    }

    public class QueryTXT : Query {
        public QueryTXT( string name ) : base(name) { QueryType = 16; }
    }

    public class QueryAAAA : Query {
        public QueryAAAA( string name ) : base(name) { QueryType = 28; }
    }

    public class QuerySRV : Query {
        public QuerySRV( string name ) : base(name) { QueryType = 33; }
    }

    class Resource {
	public string domain { get; set; }
        protected ushort resource_type;
        protected ushort resource_class = 1; // IN
        public int ttl { get; set; }

        public Resource( string s ) {
	    domain = s;
	    ttl = 2000;
        }

        public Resource( string s, ushort _type ) {
	    domain = s;
            resource_type = _type;
	    ttl = 2000;
        }

        public Resource( PacketParser packet, string s, int length ) {
	    domain = s;
#pragma warning disable 219
	    byte[] rdata = packet.PopBytes( length );
#pragma warning restore 219
        }

    }

    class TargetResource : Resource {
	public string target { get; set; }
        public TargetResource( string domain ) : base(domain) { }
        public TargetResource( string domain, ushort type )
               : base(domain, type) { }
    }

    class AddressResource : Resource {
	public IPAddress address { get; set; }
        public AddressResource( string domain ) : base(domain) { }
        public AddressResource( string domain, ushort type )
               : base(domain, type) { }
    }

    class A : AddressResource { // 1
        public A( PacketParser packet, string domain, int length )
	: base(domain, 1) {
	    byte[] rdata = packet.PopBytes( length );
	    address = new IPAddress( rdata );
        }
    }

    class CNAME : TargetResource { // 5
        public CNAME( PacketParser packet, string domain, int length )
	: base(domain, 5) {
	    target = packet.PopDomain();
        }
    }

    class PTR : TargetResource { // 12
        public PTR( PacketParser packet, string domain, int length )
	: base(domain, 12) {
	    target = packet.PopDomain();
        }
    }

    class TXT : Resource { // 16
	public string txt { get; set; }
        public TXT( PacketParser packet, string domain, int length )
	: base(domain, 16) {
	    txt = packet.PopString();
        }
    }

    class AAAA : AddressResource { // 28
        public AAAA( PacketParser packet, string domain, int length )
	: base(domain, 28) {
	    byte[] rdata = packet.PopBytes( length );
	    address = new IPAddress( rdata );
        }
    }

    class SRV : TargetResource { // 33
	public ushort port { get; set; }
	public ushort priority { get; set; }
	public ushort weight { get; set; }

        public SRV( PacketParser packet, string domain, int length )
	: base(domain, 33) {
	    priority = packet.PopUShort();
	    weight = packet.PopUShort();
	    port = packet.PopUShort();
	    target = packet.PopDomain();
        }
    }

    public class Service {
	public string domain { get; set; }
	public string target { get; set; }
	public ushort port { get; set; }
        public Service( string _domain, string _target, ushort _port ) {
	    domain = _domain;
	    target = _target;
	    port = _port;
	}
    }

    public class Message {
        protected ushort id;
        protected ushort flags = 0;

        protected ArrayList questions   = new ArrayList();
        protected ArrayList answers     = new ArrayList();
        protected ArrayList servers     = new ArrayList();
        protected ArrayList additionals = new ArrayList();

        public Message() {
            System.Random generator = new System.Random();
            id = (ushort)generator.Next(1<<16);
        }

	public ArrayList
	GetResources() {
	     ArrayList resources = new ArrayList();
	     resources.AddRange( answers );
	     resources.AddRange( servers );
	     resources.AddRange( additionals );
	     return resources;
	}

	private Resource
	PopResource( PacketParser packet ) {
	    string domain = packet.PopDomain();
	    ushort rr_type = packet.PopUShort();
#pragma warning disable 219
	    ushort rr_class = packet.PopUShort();
	    int ttl = packet.PopInt();
#pragma warning restore 219
	    ushort length = packet.PopUShort();

	    switch ( rr_type ) {
	    case  1: return new A( packet, domain, length );
	    case  5: return new CNAME( packet, domain, length );
	    case 12: return new PTR( packet, domain, length );
	    case 16: return new TXT( packet, domain, length );
	    case 28: return new AAAA( packet, domain, length );
	    case 33: return new SRV( packet, domain, length );
	    default:
		// System.Console.WriteLine( "Unknown RR ({0}/{1})", rr_type, rr_class );
	        return new Resource( packet, domain, length );
	    }

	}

	public Message( byte[] data ) {
	    PacketParser packet = new PacketParser( data );

	    id = packet.PopUShort();
	    flags = packet.PopUShort();
	    ushort question_count = packet.PopUShort();
	    ushort answer_count = packet.PopUShort();
	    ushort server_count = packet.PopUShort();
	    ushort additional_count = packet.PopUShort();

	    for ( int i = 0 ; i < question_count ; ++i ) {
#pragma warning disable 219
		string domain = packet.PopDomain();
		ushort rr_type = packet.PopUShort();
		ushort rr_class = packet.PopUShort();
#pragma warning restore 219
	    }

	    for ( int i = 0 ; i < answer_count ; ++i ) {
		answers.Add( PopResource(packet) );
	    }

	    for ( int i = 0 ; i < server_count ; ++i ) {
		servers.Add( PopResource(packet) );
	    }

	    for ( int i = 0 ; i < additional_count ; ++i ) {
		additionals.Add( PopResource(packet) );
	    }
	}

        virtual public byte[] GetBytes() {

            PacketBuilder packet = new PacketBuilder();
            packet.Add( id );
            packet.Add( flags );
            packet.Add( (ushort)questions.Count );
            packet.Add( (ushort)answers.Count );
            packet.Add( (ushort)servers.Count );
            packet.Add( (ushort)additionals.Count );

            foreach ( Query query in questions ) {
                packet.Add( query.GetBytes() );
            }

            return packet.GetBytes();
        }

        public void Transmit( Resolver socket ) {
            socket.Send( this );
        }

	public override string ToString() {
	    string result = "Response(";
	    result +=  "Q:" + questions.Count;
	    result += ",A:" + answers.Count;
	    result += ",S:" + servers.Count;
	    result += ",+:" + additionals.Count;
	    result += ")";
	    return result;
	}

	public void Write() {
	    System.Console.WriteLine( ToString() );
            foreach ( Resource resource in answers ) {
	        System.Console.WriteLine( "  " + resource.domain );
	    }
	}
    }

    public class Request : Message {

        public Request() : base() { }

        public Request( Query query ) : base() {
            Add( query );
        }

        // request A record
        public Request( string name ) : base() {
            Add( name );
        }

        public Request( byte[] data ) { }

        public void Add( Query query ) {
            questions.Add( query );
        }

        // if just ask for a name, then query for A
        // maybe also AAAA
        public void Add( string name ) {
            Add( new QueryA(name) );
        }

    }

    public class Response : Message {
	// Not yet used for anything - would only be for server
        public Response() : base() {
            flags |= 0x8000;
        }

        public Response( byte[] data ) : base(data) {
	}

    }

    struct IPv4 {
        static public IPAddress Address = IPAddress.Parse( "224.0.0.251" );
        static public IPEndPoint TransmitEndPoint = new IPEndPoint( Address, 5353 );
        static public IPEndPoint ReceiveEndPoint = new IPEndPoint( IPAddress.Any, 5353 );
    }

    struct IPv6 {
        static public IPAddress Address = IPAddress.Parse( "ff02::fb" );
        static public IPEndPoint TransmitEndpoint = new IPEndPoint( Address, 5353 );
        static public IPEndPoint ReceiveEndPoint = new IPEndPoint( IPAddress.Any, 5353 );
    }

    public class Resolver {
        UdpClient socket;
	ArrayList resources = new ArrayList();

        public Resolver() {
            socket = new UdpClient();

            socket.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
            socket.Client.Bind( IPv4.ReceiveEndPoint );

	    int ttl = 255;	// local network packets - rfc3171
	    socket.Client.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl );

	    int intfindx = BestInterfaceIndex();
	    var options = new MulticastOption( IPv4.Address, intfindx );
            socket.Client.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.AddMembership, options );
        }

        public short
        Send( Message message ) {
            byte[] data = message.GetBytes();
            socket.Send( data, data.Length, IPv4.TransmitEndPoint );
            return 0;
        }

	public static int 
	BestInterfaceIndex() {
	    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
	    foreach ( NetworkInterface nic in nics ) {
	        if ( nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet ) continue;
	        if ( nic.Supports(NetworkInterfaceComponent.IPv4) == false ) continue;
	        IPInterfaceProperties properties = nic.GetIPProperties();
	        IPv4InterfaceProperties v4_props = properties.GetIPv4Properties();
                System.Console.WriteLine( "listening on interface: {0}", nic.Name );
	        return v4_props.Index;
	    }
	    throw new SystemException("could not find an interface index");
	}

	public Response
	Receive() {
	    IPEndPoint sender = new IPEndPoint( IPAddress.Any, 0 );;
	    byte[] bytes = socket.Receive( ref sender );
	    return new Response( bytes );
	}

	public void
	GatherResponses() {
	    Response response;
	    while ( true ) {
	        response = Receive();
		resources.AddRange( response.GetResources() );
	    }
	}

	private static void
	ReceiverThread( Object arg ) {
	    Thread.CurrentThread.Name = "ResolveListenerThread";
	    Resolver receiver = (Resolver)arg;
	    try {
	        receiver.GatherResponses();
	    } catch ( System.Threading.ThreadAbortException e ) {
		// ignore
		System.Console.WriteLine( "resolver thread aborted" );
		System.Console.WriteLine( "message: {0}", e.Message );
		System.Console.WriteLine( "source: {0}", e.Source );
		System.Console.WriteLine( "stack: {0}", e.StackTrace );
		System.Console.WriteLine( "data: {0}", e.Data );
	    }
	}

	public void
	GatherResources() {
	    Thread thread = new Thread( ReceiverThread );
	    thread.Start( this );
	    Thread.Sleep(5000);
	    thread.Abort();
	}

	public string[]
	PTRs( string domain ) {
	    ArrayList ptrs = new ArrayList();
            foreach ( Resource resource in resources ) {
		if ( ! (resource is PTR) ) continue;
		if ( resource.domain.Trim('.') != domain ) continue;
		PTR ptr = (PTR)resource;
		ptrs.Add( ptr.target );
            }
	    return (string[])ptrs.ToArray( typeof(string) );
	}

	public Service[]
	SRVs( string domain ) {
	    ArrayList srvs = new ArrayList();
            foreach ( Resource resource in resources ) {
		if ( ! (resource is SRV) ) continue;
		if ( resource.domain != domain ) continue;
		SRV srv = (SRV)resource;
		srvs.Add( new Service(srv.domain, srv.target, srv.port) );
            }
	    return (Service[])srvs.ToArray( typeof(Service) );
	}

	public IPAddress[]
	As( string domain ) {
	    ArrayList addresses = new ArrayList();
            foreach ( Resource resource in resources ) {
		if ( ! (resource is A) ) {
	            System.Console.WriteLine( "resource is not A record" );
                    continue;
                }
		if ( resource.domain != domain ) {
	            System.Console.WriteLine( "resource '{0}' is not '{1}'", resource.domain, domain );
                    continue;
                }
		A a = (A)resource;
		addresses.Add( a.address );
            }
	    return (IPAddress[])addresses.ToArray( typeof(IPAddress) );
	}

	public IPAddress[]
	GetA( string domain ) {
            mDNS.Request request = new mDNS.Request( new mDNS.QueryA(domain) );
            request.Transmit( this );
	    GatherResources();
	    return As( domain );
	}

	public string[]
	GetPTR( string domain ) {
            mDNS.Request request = new mDNS.Request( new mDNS.QueryPTR(domain) );
            request.Transmit( this );
	    GatherResources();
	    return PTRs( domain );
	}

	public Service[]
	GetSRV( string domain ) {
            mDNS.Request request = new mDNS.Request( new mDNS.QuerySRV(domain) );
            request.Transmit( this );
	    GatherResources();
	    return SRVs( domain );
	}

	public IPEndPoint[]
	ResolveServiceName( string service_name ) {
	    ArrayList endpoints = new ArrayList();

	    string[] ptrs = GetPTR( service_name );
	    // System.Console.WriteLine( "Found {0} PTR RRs for {1}", ptrs.Length, service_name );

	    foreach ( string ptr in ptrs ) {
		Service[] srvs = SRVs( ptr );
		if ( srvs.Length == 0 ) {
		    srvs = GetSRV( ptr );
		}
	        // System.Console.WriteLine( "Found {0} SRV RRs for {1}", srvs.Length, ptr );

		foreach ( Service srv in srvs ) {
		    // System.Console.WriteLine( "SRV: {0}:{1}", srv.target, srv.port );
		    IPAddress[] addresses = As( srv.target );

		    if ( addresses.Length == 0 ) {
			addresses = GetA( srv.target );
		    }
	            // System.Console.WriteLine( "Found {0} A RRs for {1}", addresses.Length, srv.target );

		    foreach ( IPAddress address in addresses ) {
			// System.Console.WriteLine( "A: {0}", address );
			IPEndPoint endpoint = new IPEndPoint( address, srv.port );
			endpoints.Add( endpoint );
		    }
		}
	    }

	    return (IPEndPoint[])endpoints.ToArray( typeof(IPEndPoint) );
	}
    }

}

/* vim: set autoindent expandtab sw=4 : */
