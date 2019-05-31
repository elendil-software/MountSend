pipeline {
  agent any
  environment {
    SolutionFile = 'MountSend.sln'
    SonarqubeProjectKey = 'MountSend'
    NuGetPackageKey = '10MicronMountSend'
  }
  parameters {
    booleanParam(
      name: 'Run_SonarQube',
      defaultValue: true,
      description: 'Run SonarQube'
    )
    booleanParam(
      name: 'Publish_NuGet_Package',
      defaultValue: false,
      description: 'Publish NuGet Package'
    )
	string(
      name: 'NextVersion',
      defaultValue: '',
      description: 'Next Version'
    )
    booleanParam(
      name: 'Create_Tag',
      defaultValue: false,
      description: 'Tag'
    )
  }
  stages {
    stage('Init') {
      steps {
        powershell(script: "${env.ScriptsDir}\\PrintEnvVars.ps1", label: 'Print environment vars')
      }
    }
    stage('GitVersion') {
      steps {
	    powershell(script: "${env.ScriptsDir}\\GitVersion-SetNextVersion.ps1", label: 'Set next version')
        powershell(script: "${env.ScriptsDir}\\GitVersion.ps1", label: 'GitVersion')
        script {
          readFile('gitversion.properties').split("\r\n").each { line ->
          el = line.split("=")
          env."${el[0]}" = (el.size() > 1) ? "${el[1]}" : ""
          }
        }
        powershell(script: "${env.ScriptsDir}\\GitVersion-PrintVars.ps1", label: 'Print GitVersion vars')
        powershell(script: "${env.ScriptsDir}\\GitVersion-Update_csproj.ps1", label: 'Update *.csproj files')
      }
    }
    stage('SonarQube') {
      when {
        expression { params.Run_SonarQube == true }
      }
      steps {
        powershell(script: "${env.ScriptsDir}\\SonarQube.ps1", label: 'SonarQube')
      }
    }
    stage('Build') {
      steps {
        powershell(script: "${env.ScriptsDir}\\NuGet-Restore.ps1", label: 'NuGet Restore')
        powershell(script: "${env.ScriptsDir}\\MSBuild.ps1", label: 'MSBuild')
      }
    }
    stage('NuGet') {
      when {
        expression { params.Publish_NuGet_Package == true }
      }
      steps {
        powershell(script: "${env.ScriptsDir}\\NuGet-CreateLocalDir.ps1", label: 'Create local NuGet directory')
        powershell(script: "${env.ScriptsDir}\\NuGet-CopyNupkg.ps1", label: 'Copy *.nupkg files')
        powershell(script: "${env.ScriptsDir}\\NuGet-Push.ps1", label: 'Push *.nupkg files')
      }
    }
    stage('Tag') {
      when {
        branch 'master'
        expression { params.Create_Tag == true }
      }
      steps {
        powershell(script: "${env.ScriptsDir}\\Git-CreateTag.ps1", label: 'Tag version')
      }
    }
  }
}