"""Simple command-line sample that demonstrates service accounts.
Lists all the Google Task Lists associated with the given service account.
Service accounts are created in the Google API Console. See the documentation
for more information:
  $ python tasks.py
from oauth2client.service_account import ServiceAccountCredentials
    # Load the json format key that you downloaded from the Google API
    # Console when you created your service account. For p12 keys, use the
    # from_p12_keyfile method of ServiceAccountCredentials and specify the
    # service account email address, p12 keyfile, and scopes.
    credentials = ServiceAccountCredentials.from_json_keyfile_name(
        "service-account-abcdef123456.json",
        scopes="https://www.googleapis.com/auth/tasks",
    # Create an httplib2.Http object to handle our HTTP requests and authorize
    # it with the Credentials.
    service = build("tasks", "v1", http=http)
    # List all the tasklists for the account.
    lists = service.tasklists().list().execute(http=http)
    pprint.pprint(lists)
from django.tasks import TaskContext, task
@task()
def noop_task(*args, **kwargs):
@task
def noop_task_from_bare_decorator(*args, **kwargs):
async def noop_task_async(*args, **kwargs):
def calculate_meaning_of_life():
    return 42
def failing_task_value_error():
    raise ValueError("This Task failed due to ValueError")
def failing_task_system_exit():
    raise SystemExit("This Task failed due to SystemExit")
def failing_task_keyboard_interrupt():
    raise KeyboardInterrupt("This Task failed due to KeyboardInterrupt")
def complex_exception():
    raise ValueError(ValueError("This task failed"))
def complex_return_value():
    # Return something which isn't JSON serializable nor picklable.
    return lambda: True
def exit_task():
def hang():
    Do nothing for 5 minutes
    time.sleep(300)
def sleep_for(seconds):
@task(takes_context=True)
def get_task_id(context):
    return context.task_result.id
def test_context(context, attempt):
    assert isinstance(context, TaskContext)
    assert context.attempt == attempt
