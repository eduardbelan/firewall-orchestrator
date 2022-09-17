import re
import common
from common import list_delimiter

def normalize_svcobjects(full_config, config2import, import_id):
    svc_objects = []
    for svc_orig in full_config["serviceObjects"]:
        svc_objects.append(parse_svc(svc_orig, import_id))
    for svc_grp_orig in full_config["serviceObjectGroups"]:
        svc_grp = extract_base_svc_infos(svc_grp_orig, import_id)
        svc_grp["svc_typ"] = "group"
        parse_svc_group(svc_grp_orig, import_id, svc_objects)
        svc_objects.append(svc_grp)
    config2import['service_objects'] = svc_objects

def extract_base_svc_infos(svc_orig, import_id):
    svc = {}
    if "id" in svc_orig:
        svc["svc_uid"] = svc_orig["id"]
    else:
        svc["svc_uid"] = svc_orig["protocol"]
        if "port" in svc_orig:
            svc["svc_uid"] += "_" + svc_orig["port"] 
    if "name" in svc_orig:
        svc["svc_name"] = svc_orig["name"]
    else:
        svc["svc_name"] = svc_orig["protocol"]
        if "port" in svc_orig:
            svc["svc_name"] += "_" + svc_orig["port"] 
    if "svc_comment" in svc_orig:
        svc["svc_comment"] = svc_orig["comment"]
    svc["svc_timeout"] = None
    svc["svc_color"] = None
    svc["control_id"] = import_id 
    return svc

def parse_svc(orig_svc, import_id):
    svc = extract_base_svc_infos(orig_svc, import_id)
    svc["svc_typ"] = "simple"
    if orig_svc["type"] == "ProtocolPortObject":
        if orig_svc["protocol"] == "TCP":
            svc["svc_proto"] = 6
        elif orig_svc["protocol"] == "UDP":
            svc["svc_proto"] = 17
        elif orig_svc["protocol"] == "ESP":
            svc["svc_proto"] = 50
        # TODO add all protocols
        if "port" in orig_svc:
            if orig_svc["port"].find("-") != -1: # port range
                port_range = orig_svc["port"].split("-")
                svc["svc_port"] = port_range[0]
                svc["svc_port_end"] = port_range[1]
            else: # single port
                svc["svc_port"] = orig_svc["port"]
                svc["svc_port_end"] = None
    else:
        svc["svc_name"] += " [Not supported]" # TODO Icmp
    return svc

def parse_svc_group(orig_svc_grp, import_id, svc_objects):
    refs = []
    names = []
    if "literals" in orig_svc_grp:
        for orig_literal in orig_svc_grp["literals"]:
            literal = parse_svc(orig_literal, import_id)
            literal["svc_uid"] += "_" + orig_svc_grp["id"]
            svc_objects.append(literal)
            names.append(orig_literal["value"])
            refs.append(literal["svc_uid"])
    if "objects" in orig_svc_grp:
        for svc_orig in orig_svc_grp["objects"]:
            refs.append(svc_orig["id"])
            names.append(svc_orig["name"])
    return list_delimiter.join(refs), list_delimiter.join(names)
 