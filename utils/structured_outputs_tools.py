response = client.responses.parse(
rich.print(response)
function_call = response.output[0]
assert function_call.type == "function_call"
assert isinstance(function_call.parsed_arguments, Query)
print("table name:", function_call.parsed_arguments.table_name)
