Developer Notes:

. Remember to Set the Signing Key (Must NOT commit to Github) in Web.Config
. Remember to Remove Rewrite (so that you don't need to use https for development) in Web.Config

    <rewrite>
      <rules>
        <rule name="Redirect to HTTPS" stopProcessing="true">
          <match url=".*" />
          <conditions>
            <add input="{HTTPS}" pattern="off" ignoreCase="true" />
          </conditions>
          <action type="Redirect" redirectType="Permanent" url="https://{HTTP_HOST}/{R:1}" />
        </rule>
      </rules>
    </rewrite>

. Remember to make sure that the Firewall doesn't block the port (55506)
. Remember insert into {project root}\.vs\config\applicationhost.config the following.
	* <binding protocol="http" bindingInformation="*:55506:*" /> under the bindings section below the localhost entry
. Remember to delete the database and do an Update-Database to create and reseed a new database.
