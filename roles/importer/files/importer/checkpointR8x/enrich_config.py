#!/usr/bin/python3
import argparse, time, logging
import json, requests, requests.packages
import sys, os
sys.path.append(r"/usr/local/fworch/importer")
import fwcommon, common, getter

# requests.packages.urllib3.disable_warnings()  # suppress ssl warnings only

parser = argparse.ArgumentParser(description='Read configuration from Check Point R8x management via API calls')
parser.add_argument('-a', '--apihost', metavar='api_host', required=True, help='Check Point R8x management server')
parser.add_argument('-w', '--password', metavar='api_password_file', default='import_user_secret', help='name of the file to read the password for management server from')
parser.add_argument('-u', '--user', metavar='api_user', default='fworch', help='user for connecting to Check Point R8x management server, default=fworch')
parser.add_argument('-p', '--port', metavar='api_port', default='443', help='port for connecting to Check Point R8x management server, default=443')
parser.add_argument('-D', '--domain', metavar='api_domain', default='', help='name of Domain in a Multi-Domain Envireonment')
parser.add_argument('-l', '--layer', metavar='policy_layer_name(s)', required=True, help='name of policy layer(s) to read (comma separated)')
parser.add_argument('-x', '--proxy', metavar='proxy_string', default='', help='proxy server string to use, e.g. 1.2.3.4:8080; default=empty')
parser.add_argument('-s', '--ssl', metavar='ssl_verification_mode', default='', help='[ca]certfile, if value not set, ssl check is off"; default=empty/off')
parser.add_argument('-i', '--limit', metavar='api_limit', default='150', help='The maximal number of returned results per HTTPS Connection; default=150')
parser.add_argument('-d', '--debug', metavar='debug_level', default='0', help='Debug Level: 0(off) 4(DEBUG Console) 41(DEBUG File); default=0') 
parser.add_argument('-t', '--testing', metavar='version_testing', default='off', help='Version test, [off|<version number>]; default=off') 
parser.add_argument('-c', '--configfile', metavar='config_file', required=True, help='filename to read and write config in json format from/to')
parser.add_argument('-n', '--noapi', metavar='mode', default='false', help='if set to true (only in combination with mode=enrich), no api connections are made. Useful for testing only.')

args = parser.parse_args()
if len(sys.argv)==1:
    parser.print_help(sys.stderr)
    sys.exit(1)

with open(args.password, "r") as password_file:
    api_password = password_file.read().rstrip()

details_level = "full"    # 'standard'
use_object_dictionary = 'false'
debug_level = int(args.debug)
common.set_log_level(log_level=debug_level, debug_level=debug_level)
config = {}
starttime = int(time.time())

result = fwcommon.enrich_config (config, args.apihost, args.user, args.out, api_password, args.layer, args.package, args.domain, args.fromdate,
    args.force, args.port, { "http" : args.proxy, "https" : args.proxy }, args.limit, details_level, args.testing, debug_level, getter.set_ssl_verification(args.ssl))

duration = int(time.time()) - starttime
logging.debug ( "checkpointR8x/enrich_config - duration: " + str(duration) + "s" )

# dump new json file if config_filename is set
if args.config_filename != None and len(args.config_filename)>1:
    if os.path.exists(args.config_filename): # delete json file (to enabiling re-write)
        os.remove(args.config_filename)
    with open(args.config_filename, "w") as json_data:
        json_data.write(json.dumps(config))

sys.exit(0)
