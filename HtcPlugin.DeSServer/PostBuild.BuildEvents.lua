print("[LuaBuildEvents] Starting PostBuild\n")
require("lua.io")

-- Filename and directory blacklist
function isBlackListed(path)
  if path == [[CorePluginLoader.dll]] then return true
  elseif path == [[HtcSharp.Core.dll]] then return true
  elseif path == [[HtcSharp.Core.pdb]] then return true
  elseif path == [[HtcSharp.HttpModule.module.dll]] then return true
  elseif path == [[HtcSharp.HttpModule.module.pdb]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.Abstractions.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.Binder.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.CommandLine.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.EnvironmentVariables.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.FileExtensions.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.Json.dll]] then return true
  elseif path == [[Microsoft.Extensions.Configuration.UserSecrets.dll]] then return true
  elseif path == [[Microsoft.Extensions.DependencyInjection.Abstractions.dll]] then return true
  elseif path == [[Microsoft.Extensions.DependencyInjection]] then return true
  elseif path == [[Microsoft.Extensions.FileProviders.Abstractions.dll]] then return true
  elseif path == [[Microsoft.Extensions.FileProviders.Physical.dll]] then return true
  elseif path == [[Microsoft.Extensions.FileSystemGlobbing.dll]] then return true
  elseif path == [[Microsoft.Extensions.Hosting.Abstractions.dll]] then return true
  elseif path == [[Microsoft.Extensions.Hosting.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.Abstractions.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.Configuration.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.Console.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.Debug.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.EventLog.dll]] then return true
  elseif path == [[Microsoft.Extensions.Logging.EventSource.dll]] then return true
  elseif path == [[Microsoft.Extensions.ObjectPool.dll]] then return true
  elseif path == [[Microsoft.Extensions.Options.ConfigurationExtensions.dll]] then return true
  elseif path == [[Microsoft.Extensions.Options.dll]] then return true
  elseif path == [[Microsoft.Extensions.Primitives.dll]] then return true
  elseif path == [[Newtonsoft.Json.dll]] then return true
  elseif path == [[System.Diagnostics.EventLog.dll]] then return true
  elseif path == [[System.IO.Pipelines.dll]] then return true
  elseif path == [[Microsoft.Extensions.DependencyInjection.dll]] then return true
  else return false
  end
end

-- Create base directory
pluginsPath = Path.combine(args[2], [[HtcSharp/plugins/DeSServer]])
if Directory.exists(pluginsPath) == false then
  Directory.createDirectory(pluginsPath)
end

-- Create directories
outputDirectory = Path.combine(args[3])
outputDirectories = Directory.getDirectories(outputDirectory, "*", SearchOption.AllDirectories)
for key,value in ipairs(outputDirectories) do
  directoryReplace = value:gsub(outputDirectory, "")
  if isBlackListed(directoryReplace) == true then
    print("Blacklisted directory: " .. Path.getFileName(directoryReplace) .. "\n")
    goto continue
  end
  fixedDirectory = Path.combine(pluginsPath, directoryReplace)
  if Directory.exists(fixedDirectory) == false then
    print("Creating directory: " .. fixedDirectory .. "\n")
    Directory.createDirectory(fixedDirectory)
  end
  ::continue::
end

-- Copy files
outputFiles = Directory.getFiles(args[3], "*.*", SearchOption.AllDirectories)
for key,value in ipairs(outputFiles) do
  fileNameReplace = value:gsub(args[3], "")
  if isBlackListed(Path.getFileName(fileNameReplace)) == true then
    goto continue
  end
  fixedFileName = Path.combine(pluginsPath, fileNameReplace)
  print("Copying file: " .. fixedFileName .. "\n")
  File.copy(value, fixedFileName, true)
  ::continue::
end

print("[LuaBuildEvents] Finishing PostBuild\n")