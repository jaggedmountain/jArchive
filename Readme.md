# jArchive
> A simple file store.

This lightweight api and ui provides a great solution for ad hoc file sharing. While ideal for collaboration inside a single authentication domain, it can also host public folders that can be accessed by anyone.

## Access Control

Access permissions are applied per folder and can be designated:
* public: everyone can access
* internal: all authenticated users can access
* specified: only specified users can access

Only users specified as *Owner* or *Writer* can upload files to a folder.

## Configuration

This app leverages an OIDC identity provider for authentication, settings for which need to be provided to both the api and ui.

### API
API settings can be provided in the standard ASPNETCORE manner; a *json* file, environment args, and command-line args.
The defaults can be found at [appsettings.json](jarchive-api/appsettings.json).

Most likely, only the following need to be set:
- OpenId__Audience
- OpenId__Authority
- MaxFileSize

And optionally configure the following:
- *PathBase*: if hosting in a virtual directory
- *Cache*: if using redis to persist sharing invitations across restarts
- *Cors*: if allowing other sites to make XHR requests to the app

### UI

The user interface requires the configuration of the OIDC client.
After registering a client with an identity provider, provide a file
in `wwwroot/assets/settings.json` like so:

```json
settings: {
  appname: 'jArchive',
  apphost: '',
  oidc: {
    client_id: 'client_id',
    authority: 'https://identity_url',
    redirect_uri: 'https://app_url/oidc',
    silent_redirect_uri: 'https://app_url/assets/oidc-silent.html',
    response_type: 'code',
    scope: 'openid profile jarchive-api',
    automaticSilentRenew: true,
    monitorSession: false,
    loadUserInfo: false,
    useLocalStorage: true,
    debug: false
  }
}
```

Be sure to change *client_id*, *identity_url*, and *app_url* to match your deployment.  Also requested scope *jarchive-api* represents the api resource registered with the identity provider.  It should match the api's *OpenId__Audience* value.

## Deployment

The included docker file produces an production image which should be run as a single container. The current implementation lacks synchronization for multiple replicas, so stick with a single instance for now.

With likely few settings needed for the api, environment variables are the easiest to set.  However, if desired, mount a settings file to `/app/appsettings.json` or `/app/appsettings.Production.json`.

The ui settings file should be mounted to `/app/wwwroot/assets/settings.json`.

## Dev

Both the api and ui apps are in the repository. They only get merged when building the Docker container, where the build ui files are dropped into /app/wwwroot.

For dev using VSCode, one generally runs/debugs the api with vscode tasks/launch, and runs the angular ui in an integrated terminal with `ng serve`.

The `jarchive-ui/src/environments/environment.ts` and the `jarchive-api/appsettings.Development.json` files will need to be set with appropriate ports to ensure the ui is talking to the api, and both are talking to the identity provider.
